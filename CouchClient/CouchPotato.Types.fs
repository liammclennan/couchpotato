module CouchPotato.Types
open System
open CouchPotato.PublicTypes

type CouchMutationResponse = {ok: bool; id: string; rev:string}
type CouchGetResponse = { _id: string; _rev: string }

type CouchDocument<'t> = { _id:string; _rev: string; ``type``:string; data: 't }
    with static member createFromMutationResponse r d =
          { _id = r.id; _rev = r.rev; ``type`` = d.GetType().FullName; data = d }
         static member createFromGetResponse (r:CouchGetResponse) d =
          { _id = r._id; _rev = r._rev; ``type`` = d.GetType().FullName; data = d }

type Credentials = { username: string; password: string }
type DatabaseIdentification = {origin: Uri; database: string}
type DatabaseClient =
    | AnonymousDatabaseClient of DatabaseIdentification
    | AuthenticatingDatabaseClient of DatabaseIdentification * Credentials

type DatabaseVersion = { _id: string; _rev: string; version: string }

type DesignDocument = { language: string; views: Map<string,MapReduce> }
