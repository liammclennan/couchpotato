module CouchPotato.PublicTypes

type MapReduce = { map: string; reduce: Option<string>}
type View = { name: string; mapReduce: MapReduce }

/// Data returned from an insert or update
type CouchMutationResponse = {ok: bool; id: string; rev:string}

/// a wrapper for user data records that adds _id, _rev and type. Also
/// the format persisted in CouchDb.
type CouchDocument<'t> = { _id: string; _rev: string; ``type``:string; data: 't }
    with static member createFromMutationResponse r d =
          { _id = r.id; _rev = r.rev; ``type`` = d.GetType().FullName; data = d }
