namespace CouchPotato.Tests

open System
open NUnit.Framework
open CouchPotato.Api
open CouchPotato.PublicTypes
open FsUnit

[<TestFixture>] 
type ``views`` ()=
    let client = createDatabaseClient "http://localhost:5984" "migrationtests"

    [<Test>] 
    member x.``it should save a view`` ()=
        let view = { name = "alltemplates"; mapReduce = { map = "function (doc) { if (doc['type'] === 'Aristotle.WebModels.Template') emit(doc._id, doc); }"; reduce = None }}
        putView client "aristotle" view
        ()

    [<SetUp>]
    member x.beforeEach ()=
        deleteDatabase client
        createDatabase client
        ()

