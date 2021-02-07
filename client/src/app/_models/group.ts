export interface Group{
    name:string;
    Connections:Connection[];
}

interface Connection{
    connectionId:string;
    username:string;
}