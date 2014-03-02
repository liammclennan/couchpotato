module CouchPotato.Api
open System
open HttpClient
open CouchPotato.PublicTypes
open CouchPotato.Types
open CouchPotato.Serialization
open CouchPotato.Urls

(*
CouchPotato
=======

** Don't use this. CouchPotato is not production ready.**

Couch Potato is a .net couchdb client. 

Database Operations
========

createDatabaseClient
-----------

Creates an object that can be used to access a couchdb database. The database must exist prior to using the client object. 
eg.

    createDatabaseClient "http://localhost:5984" "testing"
*)
let createDatabaseClient url database =
    AnonymousDatabaseClient {origin=(new Uri(url)); database=database}

(*
withBasicAuthentication
-------------

Add basic authentication to a database connection.

    createDatabaseClient "http://localhost:5984" "testing"
        |> withBasicAuthentication "username" "password"
*)
let withBasicAuthentication username password (c:DatabaseClient) = 
    match c with
        | AnonymousDatabaseClient u -> AuthenticatingDatabaseClient (u, {username = username; password = password})
        | AuthenticatingDatabaseClient (u,c) -> AuthenticatingDatabaseClient (u, {username = username; password = password})

(*
ping `client`
----

Pings a couchdb server and returns basic server information. eg.

    createDatabaseClient "http://localhost:5984" "testing"
       |> ping
*)
let ping client : string = 
    (getServerUri client).ToString() |> createRequest Get |> getResponseBody
                                  
let private isSuccessResponse r =
    r.StatusCode > 199 && r.StatusCode < 300

(*
createDatabase `client`
---------------------

Creates a new database.

    createDatabase "http://localhost:5984" "testing"
*)
let createDatabase client =
    let resp = getDatabaseUriString client 
                |> createRequest Put
                |> getResponse

    if isSuccessResponse resp then
        ()
    else
        sprintf "Unable to create database %s. Status code was %d. Response was %A" (extractDatabaseIdentification client).database resp.StatusCode resp |> failwith

let deleteDatabase client =
    let resp = getDatabaseUriString client 
                |> createRequest Delete
                |> getResponse
    ()

(*
Document Operations
============

insertDocument `client` `document`
------------

Inserts a new document into a database. 

    insertDocument 
        (createDatabaseClient "http://localhost:5984" "testing") 
        { name = "Brad"; age = 93 }

Returns a `CouchDocument<'d>`.
*)
let insertDocument client (document:'d) : CouchDocument<'d> =
    let insertable = { ``type`` = document.GetType().FullName; data = document }
    let resp = getDatabaseUriString client 
                |> createRequest Post
                |> withHeader (ContentType "application/json")
                |> withBody (serializeDocument insertable)
                |> getResponse
   
    if isSuccessResponse resp then
        CouchDocument<_>.createFromMutationResponse (responseToMutationResponse(resp.EntityBody.Value)) document
    else
        sprintf "Response status code was %d" resp.StatusCode |> failwith

(*
updateDocument `client` `couchdocument`
------------------------

Updates a CouchDocument<'d>.

    let client = createDatabaseClient "http://localhost:5984" "testing"
    let inserted = insertDocument client { name = "Brad"; age = 93 }
    updateDocument client {inserted with 
        data = { inserted.data with age = 94 } }
*)
let updateDocument client (cd:CouchDocument<'d>) : CouchDocument<'d> =
    let resp = getDatabaseUriString client + "/" + cd._id
                        |> createRequest Put
                        |> withHeader (ContentType "application/json")
                        |> withBody (serializeDocument cd)
                        |> getResponse
   
    if isSuccessResponse resp then
        CouchDocument<'d>.createFromMutationResponse (responseToMutationResponse(resp.EntityBody.Value)) cd.data
    else
        sprintf "Response status code was %d" resp.StatusCode |> failwith

(*
getDocument `client` `id`
------------

Retrieve a `CouchDocument<'d>` by its id.

    getDocument client "id of the document"
*)
let getDocument client (id:string) : CouchDocument<'t> =
    let resp = getDatabaseUriString client + "/" + id
                |> createRequest Get
                |> withHeader (ContentType "application/json")
                |> getResponse

    if isSuccessResponse resp then
        responseToCouchDocument<'t> resp.EntityBody.Value
    else
        sprintf "Response status code was %d" resp.StatusCode |> failwith

(*
View Operations
===============

putView `client` `designDocName` `view`
------

Add a new to a design doc. Note: currently this function will cause an existing design doc to be overridden.

    let view = { 
                name = "alltemplates"; 
                mapReduce = { 
                             map = "function (doc) { 
                                        if (doc['type'] === 'CouchPotato.Tests.Thing') {
                                            emit(doc._id, doc); 
                                        }"; 
                             reduce = None }}
    putView client "nameofdesigndoc" view
*)
let putView client designDocName (view:View) =
    let mr = seq {
                yield view.name, view.mapReduce
             } |> Map.ofSeq
    let couchView = { language = "javascript"; views = mr }
    
    let req = (getDocumentUri client ("_design/" + designDocName)).ToString()
                    |> createRequest Put
                    |> withHeader (ContentType "application/json")
                    |> withBody (serializeDocument couchView)
    let resp = getResponse req
    if isSuccessResponse resp then
        ()
    else
        sprintf "Response status code was %d" resp.StatusCode |> failwith

(*
queryView `client` `viewDoc` `viewName`
----------------------

Query a view for all results.

    queryView<'Thing> client "viewtests" "alltemplates"

Returns a sequence of CouchDocuments<'d>
*)
let queryView<'d> client viewDoc viewName : seq<CouchDocument<'d>> =
    let resp = getViewUri client viewDoc viewName
                |> createRequest Get
                |> withHeader (ContentType "application/json")
                |> getResponse

    if isSuccessResponse resp then
        responseToSeq resp.EntityBody.Value
    else
        sprintf "Response status code was %d" resp.StatusCode |> failwith

// this is here because the client needs this type
type MigrationStep = { name: string; action: DatabaseClient -> Unit }

(*
Migration Operations
====================

currentVersion `client`
----------------

Fetch the current schema version.

    currentVersion client

Returns an int.
*)
let currentVersion client =
    let resp = getDatabaseUriString client + "/" + "migration_version"
                |> createRequest Get
                |> withHeader (ContentType "application/json")
                |> getResponseBody
    let o = responseToDocument<DatabaseVersion> resp
    o.version

(*
migrateTo `client` `v` `steps`
-----------

Migrate the database forward to version `v`.

    migrateTo client "001" steps
*)
let migrateTo client (v:string) (steps: List<MigrationStep>)=
    let presentVersion = currentVersion client
    let stepsToApply = List.filter (fun s -> s.name <= v && s.name > presentVersion) steps |> List.sortBy (fun s -> s.name)
    if presentVersion < v && stepsToApply.Length > 0 then 
        for { name = name; action = action } in stepsToApply do
            action client
        let resp = (getDocumentUri client "migration_version").ToString()
                    |> createRequest Put
                    |> withHeader (ContentType "application/json")
                    |> withBody ("{\"version\": \"" + stepsToApply.[stepsToApply.Length-1].name + "\"}")
                    |> getResponse
        ()
    ()


