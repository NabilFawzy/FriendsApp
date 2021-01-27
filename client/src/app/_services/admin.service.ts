import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../_models/User';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
   baseUrL=environment.apiUrl;
  constructor(private http:HttpClient) { }

  getUsersWithRoles(){
     return this.http.get<Partial<User[]>>(this.baseUrL+'admin/users-with-roles');
  }
  updateUserRoles(username:string,roles:string[]){
    return this.http.post(this.baseUrL+'admin/edit-roles/'+username+'?roles='+roles,{});

  }
}
