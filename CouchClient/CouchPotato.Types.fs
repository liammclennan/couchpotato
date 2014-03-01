module CouchPotato.Types
open System
open CouchPotato.PublicTypes

/// format of a row returned from a couchdb index
type CouchListRow<'t> = { id: string; key: string; value: CouchDocument<'t> }

/// format returned from a couchdb index
type CouchListResponse<'t> = { total_rows: int; offset: int; rows: seq<CouchListRow<'t>> }

type Credentials = { username: string; password: string }
type DatabaseIdentification = {origin: Uri; database: string}
type DatabaseClient =
    | AnonymousDatabaseClient of DatabaseIdentification
    | AuthenticatingDatabaseClient of DatabaseIdentification * Credentials

// record used to persist the database version. Used for migrations.
type DatabaseVersion = { _id: string; _rev: string; version: string }

type DesignDocument = { language: string; views: Map<string,MapReduce> }

type CouchInsertDocument<'t> = { ``type``:string; data: 't }