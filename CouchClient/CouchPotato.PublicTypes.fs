module CouchPotato.PublicTypes

type MapReduce = { map: string; reduce: Option<string>}
type View = { name: string; mapReduce: MapReduce }