namespace Green

type Func = obj array -> obj

type RuntimeException(message) = inherit System.Exception(message)

type Module = {name:string; globals:Map<string,obj>}
