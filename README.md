CouchPotato
===========

Couch Potato is a .net couchdb client. 

Supported Operations
-------------------

### createDatabaseClient

Creates an object that can be used to access a couchdb database. The database must exist prior to using the client object. 

    createDatabaseClient <server url> <database name>

eg.

    createDatabaseClient "http://localhost:5984" "testing"

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