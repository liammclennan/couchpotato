module CouchPotato.Serialization
open Newtonsoft.Json
open CouchPotato.PublicTypes
open CouchPotato.Types

let serializeDocument = JsonConvert.SerializeObject

let responseToCouchDocument<'r> resp =
     JsonConvert.DeserializeObject<CouchDocument<'r>>(resp)

let responseToMutationResponse resp = 
    JsonConvert.DeserializeObject<CouchMutationResponse>(resp)

let responseToDocument<'r> r = JsonConvert.DeserializeObject<'r> r

let responseToSeq<'r> resp : seq<CouchDocument<'r>> =
    let results = JsonConvert.DeserializeObject<CouchListResponse<'r>>(resp)
    results.rows
        |> Seq.map (fun (record) -> record.value)
