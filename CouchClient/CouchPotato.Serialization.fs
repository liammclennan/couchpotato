module CouchPotato.Serialization
open Newtonsoft.Json
open CouchPotato.Types

/// add a 'type' property and convert a record into a json string
let serializeRecord o = 
    let jo = JsonConvert.SerializeObject o
                |> JsonConvert.DeserializeObject
                :?> Newtonsoft.Json.Linq.JObject
    jo.Add(new Newtonsoft.Json.Linq.JProperty("type", o.GetType().FullName))
    JsonConvert.SerializeObject(jo)

let serializeView v =
    JsonConvert.SerializeObject(v)

let serializeCouchDocument (cd:CouchDocument<'d>) =
    let jo = JsonConvert.SerializeObject cd.data
                |> JsonConvert.DeserializeObject
                :?> Newtonsoft.Json.Linq.JObject
    jo.Add(new Newtonsoft.Json.Linq.JProperty("type", typeof<'d>.FullName))
    jo.Add(new Newtonsoft.Json.Linq.JProperty("_id", cd._id))
    jo.Add(new Newtonsoft.Json.Linq.JProperty("_rev", cd._rev))
    JsonConvert.SerializeObject(jo)

let responseToMutationResponse resp = 
    JsonConvert.DeserializeObject<CouchMutationResponse>(resp)

let responseToGetResponse resp =
    JsonConvert.DeserializeObject<CouchGetResponse>(resp)

let responseToRecord<'r> resp =
    JsonConvert.DeserializeObject<'r>(resp)