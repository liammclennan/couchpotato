﻿namespace CouchPotato.Tests

open System
open NUnit.Framework
open CouchPotato.Api
open CouchPotato.Urls
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

    [<Test>] 
    member x.``it should insert the document`` ()=
        let d = insertDocument client { name = "Alice"; age = 11 }
        d.data.name |> should equal "Alice"

    [<Test>] 
    member x.``it should save discriminated unions and Tuples`` ()=
        let d = insertDocument client (A ("cats",7))
        match d.data with
            | A (s,n) -> s |> should equal "cats" 
            | _ -> Assert.Fail "This ain't right"

[<TestFixture>]
type ``when reading a document by its key`` ()=
    let client = createDatabaseClient "http://localhost:5984" "testing"
    let d= insertDocument client { name = "Brad"; age = 93 }

    [<Test>] 
    member x.``it should return the object`` ()=
        let d' = getDocument client d._id
        d'.data.name |> should equal "Brad"

[<TestFixture>]
type ``when updating a document`` ()=
    let client = createDatabaseClient "http://localhost.:5984" "testing"
    let d = insertDocument client { name = "Update Me"; age = 63 }
                
    [<Test>] 
    member x.``it should be able to modify the document`` ()=
        let d' = updateDocument client {d with data = { d.data with age = 33 } }
        d'._rev |> should not' (equal d._rev)

[<TestFixture>]
type ``when querying`` ()=
    let client = createDatabaseClient "http://localhost.:5984" "testing"

    [<Test>]
    member x.``it should be able to query a new view`` ()=
        insertDocument client { name = "Alice"; age = 11 } |> ignore
        insertDocument client { name = "Bob"; age = 8 } |> ignore
//        insertDocument client {  } 
//        getViewUri client "views" "byname" |> should equal "cat"