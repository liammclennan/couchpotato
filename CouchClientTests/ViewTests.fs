namespace CouchPotato.Tests

open System
open NUnit.Framework
open CouchPotato.Api
open CouchPotato.PublicTypes
open FsUnit

type Thing = { value: string }

[<TestFixture>] 
type ``views`` ()=
    let client = createDatabaseClient "http://localhost:5984" "testing"

    [<Test>] 
    member x.``it should save a view`` ()=
        let view = { name = "alltemplates"; mapReduce = { map = "function (doc) { if (doc['type'] === 'CouchPotato.Tests.Thing') emit(doc._id, doc); }"; reduce = None }}
        putView client "viewtests" view
        ()

    [<Test>]
    member x.``it should query a view`` ()=
        let doc = { value = "foo" }
        insertDocument client doc |> ignore
        x.``it should save a view``()
        let (r:seq<CouchDocument<Thing>>) = queryView<'Thing> client "viewtests" "alltemplates"
        Seq.length r |> should equal 1
        (Seq.head r).data |> should equal doc
        ()


    [<SetUp>]
    member x.beforeEach ()=
        deleteDatabase client
        createDatabase client
        ()

