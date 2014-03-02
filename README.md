CouchPotato
===========

** Don't use this. CouchPotato is not production ready.**

Couch Potato is a .net couchdb client. 

Supported Operations
-------------------

### Database Operations

    createDatabaseClient <server url> <database name>

Creates an object that can be used to access a couchdb database. The database must exist prior to using the client object. 

eg.

    createDatabaseClient "http://localhost:5984" "testing"

    createDatabaseClient "http://localhost:5984" "testing"
        |> withBasicAuthentication "username" "password"

    createDatabaseClient "http://localhost:5984" "testing"
        |> ping

Pings a couchdb server and returns basic server information.





### insertDocument

Inserts a new document into a database.

    insertDocument <client> <document>

eg.

    insertDocument (createDatabaseClient "http://localhost:5984" "testing") { name = "Brad"; age = 93 }

### getDocument

Retrieve a single document by it's Id.

    getDocument <client> <id>

eg.

	getDocument client "e3f68ed8c9432c13dc7ea87308005037"


Running the Tests
----------------

To run the tests

* have a instance of couchdb running on localhost:5984 that allows anonymous authentication.