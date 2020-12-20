import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { User } from '../_models/User';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
   baseUrl='https://localhost:5001/api/';
   //ReplaySubject buffer object store value for object with number of objects()
   
   private currentUserSource = new ReplaySubject<User>(1);
   currentUser$=this.currentUserSource.asObservable();//currentUser$ dollar because it is Observable
  constructor(private http:HttpClient) { }

  //pipe for rxJs observeable
  login(model:any){
    return this.http.post<User>(this.baseUrl+'account/login',model).pipe(
      map((response:User)=>{
        const user=response;
        if(user){
          localStorage.setItem('user',JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    );
  }
  setCurrentUser(user:User){
    this.currentUserSource.next(user);
  }

  register(model:any){
    return this.http.post<User>(this.baseUrl+'account/register',model).pipe(
      map((response:User)=>{
        const user=response;
        if(user){
          localStorage.setItem('user',JSON.stringify(user));
          this.currentUserSource.next(user);
        }
        //return user;
      })
    );
  }
  

  

  logout(){
    localStorage.removeItem('user');
   
    this.currentUserSource.next(null);
  }
}