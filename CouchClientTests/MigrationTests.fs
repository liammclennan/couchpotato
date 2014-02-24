namespace CouchPotato.Tests

open System
open NUnit.Framework
open CouchPotato.Api
open FsUnit

type Person2 = { name: string; age: int }

[<TestFixture>] 
type ``migrations`` ()=
    let client = createDatabaseClient "http://localhost:5984" "migrationtests"

    [<Test>]
    member x.``it should apply a first migration`` ()=
        let steps = [{ name="001"; action = fun c -> insertDocument client { name = "Alice"; age = 11 } |> ignore}]
        migrateTo client "001" steps
        currentVersion client |> should equal "001"

    [<Test>]
    member x.``it should not apply too many migrations`` ()=
        let steps = [{ name="001"; action = fun c -> insertDocument client { name = "Alice"; age = 11 } |> ignore}
                     { name="002"; action = fun c -> insertDocument client { name = "Bob"; age = 9 } |> ignore}
                     { name="003"; action = fun c -> insertDocument client { name = "Bill"; age = 63 } |> ignore}
                        ]
        migrateTo client "002" steps
        currentVersion client |> should equal "002"

    [<SetUp>]
    member x.beforeEach ()=
        deleteDatabase client
        createDatabase client
        ()
