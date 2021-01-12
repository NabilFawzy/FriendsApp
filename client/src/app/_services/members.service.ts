import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/Member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/User';
import { UserParams } from '../_modules/userParams';
import { AccountService } from './account.service';




@Injectable({
  providedIn: 'root'
})
export class MembersService {
  members:Member[]=[];//for cache members
  baseUrl=environment.apiUrl;
  memberCache=new Map();
  user:User;
  userParams:UserParams;
  
  constructor(private http:HttpClient,private accountService:AccountService) {
    accountService.currentUser$.pipe(take(1)).subscribe(user=>{
      this.user=user;
      this.userParams=new UserParams(this.user);
      //console.log(this.userParams.gender)
     }
     );
   }
   setUserParams(params:UserParams){
     this.userParams=params;
   }
   getUserParams(){
     return this.userParams;
   }
   resetUserParams(){
    this.userParams=new UserParams(this.user);
     return this.userParams;
   }


  getMembers(userParams:UserParams){
    //if(this.members.length>0) return of(this.members)  //of => to return observable
    
    var response=this.memberCache.get(Object.values(userParams).join('-'));
     if(response) return of(response)

    let params=this.getPaginationHeaders(userParams)
    
    return this.getPaginationResult<Member[]>(this.baseUrl+ 'users',params)
    .pipe(map(response=>{
      this.memberCache.set(Object.values(userParams).join('-'),response);
      return response;
    }))
  }

  private getPaginationResult<T>(url:any,params:any) {
    const paginatedResult:PaginatedResult<T>=new PaginatedResult<T>();
    return this.http.get<T>(url , { observe: 'response', params }).pipe(
      map(response => {
        paginatedResult.result = response.body; //set result
        if (response.headers.get('Pagination') !== null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
       
        return paginatedResult;
      })
    );
  }

  private getPaginationHeaders(userParams:UserParams){
     //if(this.members.length>0) return of(this.members)  //of => to return observable
     let params=new HttpParams();//for serialize params and add to query string
       
       params=params.append('pageNumber',userParams.pageNumber.toString());
       params=params.append('pageSize',userParams.pageSize.toString());
       params=params.append('minAge',userParams.minAge.toString());
       params=params.append('maxAge',userParams.maxAge.toString());
       params=params.append('gender',userParams.gender);
       params=params.append('orderBy',userParams.orderBy);
       

       return params;
     
  }

  getMember(username:string){
    //const member=this.members.find(x=>x.userName===username);
    //if(member!=undefined) return of(member)

    //array of array from users so we use reduce function make one array hold all members 
    const member=[...this.memberCache.values()]
                 .reduce((arr,ele)=>arr.concat(ele.result),[])
                 .find((member:Member)=>member.userName==username)
         // console.log(member);
    
    if(member) return of(member);

    return this.http.get<Member>(this.baseUrl+'users/'+username);
  }
  updateMember(member:Member){
    return this.http.put(this.baseUrl+'users',member).pipe(
      map(()=>{
        const index=this.members.indexOf(member);
        this.members[index]=member
      })
    );
  }

 

  setMainPhoto(photoId:Number){
    return this.http.put(this.baseUrl+'users/set-main-photo/'+photoId,{});
  }

  deletePhoto(photoId:Number){
    return this.http.delete(this.baseUrl+'users/delete-photo/'+photoId,{});
  }

}
