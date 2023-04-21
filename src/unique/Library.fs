// Implementation of version 5 from https://tools.ietf.org/html/rfc4122
// The requirements for these types of UUIDs are as follows:
//
//   o  The UUIDs generated at different times from the same name in the
//      same namespace MUST be equal.
//
//   o  The UUIDs generated from two different names in the same namespace
//      should be different (with very high probability).
//
//   o  The UUIDs generated from the same name in two different namespaces
//      should be different with (very high probability).
//
//   o  If two UUIDs that were generated from names are equal, then they
//      were generated from the same name in the same namespace (with very
//      high probability).
// The algorithm for generating a UUID from a name and a name space are as follows:
//
//   1. Allocate a UUID to use as a "name space ID" for all UUIDs
//      generated from names in that name space; see Appendix C for some
//      pre-defined values.
//
//   2. Choose either MD5 [4] or SHA-1 [8] as the hash algorithm; If
//      backward compatibility is not an issue, SHA-1 is preferred.
//
//   3. Convert the name to a canonical sequence of octets (as defined by
//      the standards or conventions of its name space); put the name
//      space ID in network byte order.
//
//   4. Compute the hash of the name space ID concatenated with the name.
//
//   5. Set octets zero through 3 of the time_low field to octets zero
//      through 3 of the hash.
//
//   6. Set octets zero and one of the time_mid field to octets 4 and 5 of
//      the hash.
//
//   7. Set octets zero and one of the time_hi_and_version field to octets
//      6 and 7 of the hash.
//
//   8. Set the four most significant bits (bits 12 through 15) of the
//      time_hi_and_version field to the appropriate 4-bit version number
//      from Section 4.1.3.
//
//   9. Set the clock_seq_hi_and_reserved field to octet 8 of the hash.
//
//  10. Set the two most significant bits (bits 6 and 7) of the
//      clock_seq_hi_and_reserved to zero and one, respectively.
//
//  11. Set the clock_seq_low field to octet 9 of the hash.
//
//  12. Set octets zero through five of the node field to octets 10
//      through 15 of the hash.
//
//  13. Convert the resulting UUID to local byte order.

namespace Unique

module String =
    open System
    open System.Text
    
    let toBytes (s:string) = Encoding.UTF8.GetBytes(s)
    let fromBytes (bs:byte[]) = Encoding.UTF8.GetString(bs)
    let fromGuid (guid:Guid) = guid.ToString()
    
module NS =
    open System
    
    [<Literal>]
    let private dns = "6ba7b810-9dad-11d1-80b4-00c04fd430c8"
    [<Literal>]
    let private url = "6ba7b811-9dad-11d1-80b4-00c04fd430c8"
    [<Literal>]
    let private oid = "6ba7b812-9dad-11d1-80b4-00c04fd430c8"
    [<Literal>]
    let private x500 = "6ba7b814-9dad-11d1-80b4-00c04fd430c8"
    
    type Namespace =
        | DNS
        | URL
        | OID
        | X500
        | Custom of System.Guid
    let memoize fn =
        let cache = System.Collections.Generic.Dictionary<_,_>()
        (fun x ->
        match cache.TryGetValue x with
        | true, v -> v
        | false, _ -> let v = fn (x)
                      cache.Add(x,v)
                      v)   
    let private toGuid' = function
        | DNS -> Guid.Parse(dns)
        | URL -> Guid.Parse(url)
        | OID -> Guid.Parse(oid)
        | X500 -> Guid.Parse(x500)
        | Custom guid -> guid
    
    let toGuid = memoize toGuid'

module Hash =
    open System.Security.Cryptography
    
    type Algorithm =
        | SHA1
        | MD5
    let private md5Hasher (bs:byte[]) =
        use hasher = MD5.Create()
        hasher.ComputeHash(bs)
        
    let private sha1Hasher (bs:byte[]) =
        use hasher = SHA1.Create()
        hasher.ComputeHash(bs)
    
    let makeHasher algorithm =
        match algorithm with
        | SHA1 -> sha1Hasher
        | MD5 -> md5Hasher

    let version = function
        | SHA1 -> 5
        | MD5 -> 3

module NamedGuid =
    open System
    
    let empty = Guid.Empty
    let toString (guid:Guid) = guid.ToString()
    let toByteArray (guid:Guid) = guid.ToByteArray()
  
    let private swapBytes (lhs, rhs) (bs:byte[]) =
        let temp = bs.[lhs]
        bs.[lhs] <- bs.[rhs]
        bs.[rhs] <- temp
        bs
        
    let private orderBytes (bs:byte[]) =
        bs
        |> swapBytes (0, 3)
        |> swapBytes (1, 2)
        |> swapBytes (4, 5)
        |> swapBytes (6, 7)
    
    let private genGuid (version:int) (hash:byte[]) : byte[] =
        
        let initializer (i:int) : byte =
            match i with
            //   8. Set the four most significant bits (bits 12 through 15) of the
            //      time_hi_and_version field to the appropriate 4-bit version number
            //      from Section 4.1.3.
            | 6 -> (hash.[6] &&& (byte 0x0F)) ||| (byte version <<< 4)
            //  10. Set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively.
            | 8 -> (hash.[8] &&& (byte 0x3F)) ||| (byte 0x80)
            // Other steps are just copying like for like
            | _ -> hash.[i]
            
        Array.init 16 initializer
        
    // pos 7 is most significant, 0 is least
    let private is1 pos b = (b &&& (1 <<< pos)) <> 0
    let version (guid:System.Guid) =
        // time_hi_and_version in bytes 6 - 7
        let bs = guid.ToByteArray() |> orderBytes
        let b = bs.[6] |> int
        let bitsSet = [is1 7 b;is1 6 b;is1 5 b;is1 4 b;]
        //Msb0  Msb1  Msb2  Msb3   Version     Description
        //0     0     0     1        1         The time-based version specified in this document.
        //0     0     1     0        2         DCE Security version, with embedded POSIX UIDs.
        //0     0     1     1        3         The name-based version specified in this document that uses MD5 hashing.
        //0     1     0     0        4         The randomly or pseudo-randomly generated version specified in this document.
        //0     1     0     1        5         The name-based version specified in this document that uses SHA-1 hashing.
        match bitsSet with
        | [false;false;false;true] -> 1
        | [false;false;true;false] -> 2
        | [false;false;true;true]  -> 3
        | [false;true;false;false] -> 4
        | [false;true;false;true]  -> 5
        | _ -> 0
        
    let newGuidFromBytes algorithm ns (name:string) =
        let version = Hash.version algorithm
        //Step 1: Allocate a UUID to use as a "name space ID". 3b. Put the name space ID in network byte order.
        let nsBs = ns|> NS.toGuid |> toByteArray |> orderBytes
        //Step 2: Choose either MD5 [4] or SHA-1 [8] as the hash algorithm
        let hasher = Hash.makeHasher algorithm
        //Step 3: Convert the name to a canonical sequence of octets
        let nameBs = name |> String.toBytes
        // Step 4: Compute the hash of the name space ID concatenated with the name
        let hash = Array.append nsBs nameBs |> hasher
        // Steps 5+
        hash |> genGuid version
        // Step 13: Convert the resulting UUID to local byte order.
        |> orderBytes
        
    let newGuid algorithm ns (name:string) =
        newGuidFromBytes algorithm ns name |> Guid
        
namespace Unique.CSharp

    module SpaceId =
        open Unique
        let DNS = NS.DNS |> NS.toGuid
        let URL = NS.URL |> NS.toGuid
        let OID = NS.OID |> NS.toGuid
        
    module NamedGuid =
        open Unique.NS
        
        let NewGuid nsGuid name =
            match nsGuid with
            | g when g = (DNS |> toGuid) -> Unique.NamedGuid.newGuid Unique.Hash.SHA1 Unique.NS.DNS name
            | g when g = (URL |> toGuid) -> Unique.NamedGuid.newGuid Unique.Hash.SHA1 Unique.NS.URL name
            | g when g = (OID |> toGuid) -> Unique.NamedGuid.newGuid Unique.Hash.SHA1 Unique.NS.OID name
            | _ -> Unique.NamedGuid.newGuid Unique.Hash.SHA1 (Unique.NS.Custom nsGuid) name
            
        let Version guid = Unique.NamedGuid.version guid
        