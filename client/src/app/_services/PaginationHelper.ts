import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs/operators";
import { PaginatedResult } from "../_models/pagination";

  export function getPaginationResult<T>(url:any,params:any,http:HttpClient) {
    const paginatedResult:PaginatedResult<T>=new PaginatedResult<T>();
    return http.get<T>(url , { observe: 'response', params }).pipe(
      map(response => {
        paginatedResult.result = response.body; //set result
        if (response.headers.get('Pagination') !== null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
       
        return paginatedResult;
      })
    );
  }

  export function getPaginationHeaders(pageNumber:number,pageSize:number){
     //if(this.members.length>0) return of(this.members)  //of => to return observable
     let params=new HttpParams();//for serialize params and add to query string
       
       params=params.append('pageNumber',pageNumber.toString());
       params=params.append('pageSize',pageSize.toString());
      
       

       return params;
     
  }