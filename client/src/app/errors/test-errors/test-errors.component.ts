import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent implements OnInit {
  validationErrors:string[]
  baseUrl=environment.apiUrl;
  constructor(private http:HttpClient) { }

  ngOnInit(): void {
  }
  get404Erorr(){
    this.http.get(this.baseUrl+'buggy/not-found').subscribe(
      response=>{
        console.log(response)
      },
      error=>{
        console.log(error)
      }
    );
  }

  get400Erorr(){
    this.http.get(this.baseUrl+'buggy/bad-request').subscribe(
      response=>{
        console.log(response)
      },
      error=>{
        console.log(error)
      }
    );
  }


  get500Erorr(){
    this.http.get(this.baseUrl+'buggy/server-error').subscribe(
      response=>{
        console.log(response)
      },
      error=>{
        console.log(error)
      }
    );
  }


  get401Erorr(){
    this.http.get(this.baseUrl+'buggy/auth').subscribe(
      response=>{
        console.log(response)
      },
      error=>{
        console.log(error)
      }
    );
  }

  get400ValidationErorr(){
    this.http.post(this.baseUrl+'account/register',{}).subscribe(
      response=>{
        console.log(response)
      },
      error=>{
        console.log(error)
        this.validationErrors=error;
      }
    );
  }

}
