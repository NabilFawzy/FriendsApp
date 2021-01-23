import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/Message';
import { getPaginationHeaders, getPaginationResult } from './PaginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl=environment.apiUrl;
  constructor(private http:HttpClient) { 
  }

  getMessages(pageNumber:number,pageSize:number,Container:string){
    let params=getPaginationHeaders(pageNumber,pageSize);
    params=params.append('Container',Container);
 
    return getPaginationResult<Message[]>(this.baseUrl+ 'messages',params,this.http);
  }
  deleteMessage(id:number){
    return this.http.delete(this.baseUrl+'messages/'+id);
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }
  
  sendMessage(username:string,content:string){
    console.log(username)
    console.log(content)
    return this.http.post<Message>(this.baseUrl+'messages',{RecipentUsername:username,content})
  }
}
