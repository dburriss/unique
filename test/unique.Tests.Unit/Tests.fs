module Tests

open System
open Unique.Hash
open Xunit
open Unique
open Swensen.Unquote

[<Fact>]
let ``version 4 uuid for System.Guid.NewGuid()`` () =
    let guid = Guid.NewGuid()
    let version = Guid.version guid
    test <@ version = 4 @>

// from http://www.rfc-editor.org/errata_search.php?rfc=4122
[<Fact>]
let ``version 3 uuid for www.example.com`` () =
    let expected = Guid.Parse("5df41881-3aed-3515-88a7-2f4a814cf09e")
    let result = Guid.namedGuid Algorithm.MD5 NS.DNS "www.example.com"
    test <@ result = expected  @>
    
[<Fact>]
let ``version returns 3 for MD5`` () =
    let guid = Guid.Parse("5df41881-3aed-3515-88a7-2f4a814cf09e")
    let version = Guid.version guid
    test <@ version = 3 @>

// from https://www.uuidtools.com/v5
[<Fact>]
let ``version 5 uuid for devonburriss.me`` () =
    let expected = Guid.Parse("556deb59-9074-5490-93fd-78e6bd319481")
    let result = Guid.namedGuid Algorithm.SHA1 NS.DNS "devonburriss.me"
    test <@ result = expected @>
    
[<Fact>]
let ``version returns 5 for SHA1`` () =
    let guid = Guid.Parse("556deb59-9074-5490-93fd-78e6bd319481")
    let version = Guid.version guid
    test <@ version = 5 @>

[<Fact>]
let ``sanity check`` () =
    let dnsGuid = NS.DNS |> NS.toGuid
    let expected = Guid.namedGuid Algorithm.SHA1 NS.DNS "devonburriss.me"
    let result = System.Guid.namedGuid dnsGuid  "devonburriss.me"
    test <@ result = expected @>