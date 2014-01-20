namespace CouchPotato.Tests

open System
open NUnit.Framework
open CouchPotato.Api
open FsUnit

[<TestFixture>] 
type ``create a client`` ()=

    [<Test>] member x.
     ``it should provide a database uri`` ()=
        createDatabaseClient "http://localhost:5984/" "testing"
            |> getDatabaseUri
            |> (fun u -> u.ToString())
            |> should equal "http://localhost:5984/testing"

    [<Test>] member x.
     ``it should provide a document uri`` ()=
        getDocumentUri (createDatabaseClient "http://localhost:5984/" "testing") "AliceXYZ"
            |> (fun u -> u.ToString())
            |> should equal "http://localhost:5984/testing/AliceXYZ"

[<TestFixture>]
type ``when pinged`` ()=
    let client = createDatabaseClient "http://localhost:5984" "testing"

    [<Test>] member x.
     ``it should return basic server information`` ()=
        ping client |> should startWith """{"couchdb":"Welcome"""

type Person = { name: string; age: int }
type AOrB = 
            | A of string * int
            | B of int * bool

[<TestFixture>]
type ``when inserting a document`` ()=
    let client = createDatabaseClient "http://localhost:5984" "testing"

    [<Test>] member x.
     ``it should insert the document`` ()=
        match insertDocument client { name = "Alice"; age = 11 } with
            | Choice2Of2(s) -> Assert.Fail s
            | Choice1Of2(d) -> d.data.name |> should equal "Alice"
    
    [<Test>] member x.
     ``it should save discriminated unions and Tuples`` ()=        
        match insertDocument client (A ("cats",7)) with
           | Choice2Of2(s) -> Assert.Fail s
           | Choice1Of2(d) -> 
                match d.data with
                    | A (s,n) -> s |> should equal "cats" 
                    | _ -> Assert.Fail "This ain't right"

[<TestFixture>]
type ``when reading a document by its key`` ()=
    let client = createDatabaseClient "http://localhost:5984" "testing"
    let doc = match insertDocument client { name = "Brad"; age = 93 } with
                | Choice2Of2(s) -> failwith s
                | Choice1Of2(d) -> d

    [<Test>] member x.``it should return the object`` ()=
                match getDocument client doc._id with
                    | Choice2Of2(s) -> failwith s
                    | Choice1Of2(d) -> d.data.name |> should equal "Brad"

[<TestFixture>]
type ``when updating a document`` ()=
    let client = createDatabaseClient "http://localhost.:5984" "testing"
    let doc = match insertDocument client { name = "Update Me"; age = 63 } with
                | Choice2Of2(s) -> failwith s
                | Choice1Of2(d) -> d
                
    [<Test>] 
    member x.``it should be able to modify the document`` ()=
        match updateDocument client {doc with data = { doc.data with age = 33 } } with
            | Choice2Of2(s) -> failwith s
            | Choice1Of2(d) ->
                d._rev |> should not' (equal doc._rev)

[<TestFixture>]
type ``when querying`` ()=
    let client = createDatabaseClient "http://localhost.:5984" "testing"

    [<Test>]
    member x.``it should be able to query a new view`` ()=
        let view = { View.name = "view1"; mapReduce = {map = ""; reduce = None } }
        queryView client view "startkey" "endkey"