import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/User';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
   baseUrl=environment.apiUrl;
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
          this.setCurrentUser(user);
        }
      })
    );
  }
  setCurrentUser(user:User){
    localStorage.setItem('user',JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  register(model:any){
    return this.http.post<User>(this.baseUrl+'account/register',model).pipe(
      map((response:User)=>{
        const user=response;
        if(user){
          this.setCurrentUser(user);
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
