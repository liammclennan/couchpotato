module CouchPotato.Types
open System
open CouchPotato.PublicTypes

type CouchListRow<'t> = { id: string; key: string; value: CouchDocument<'t> }
type CouchListResponse<'t> = { total_rows: int; offset: int; rows: seq<CouchListRow<'t>> }

type Credentials = { username: string; password: string }
type DatabaseIdentification = {origin: Uri; database: string}
type DatabaseClient =
    | AnonymousDatabaseClient of DatabaseIdentification
    | AuthenticatingDatabaseClient of DatabaseIdentification * Credentials

type DatabaseVersion = { _id: string; _rev: string; version: string }

type DesignDocument = { language: string; views: Map<string,MapReduce> }

type CouchInsertDocument<'t> = { ``type``:string; data: 't }