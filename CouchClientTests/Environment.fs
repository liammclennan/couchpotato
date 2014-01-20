namespace CouchPotato.Tests

open NUnit.Framework
open CouchPotato.Api

[<SetUpFixture>]
type Environment ()=

    [<SetUp>]
    member this.RunBeforeAnyTests ()=
        let client = createDatabaseClient "http://localhost:5984" "testing"
        deleteDatabase client
        createDatabase client

    [<TearDown>]
    member this.RunAfterAnyTests ()=
        ()