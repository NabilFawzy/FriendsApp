import { HttpClient } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
 
  RegisterMode=false;
  users:any;
  constructor(private http:HttpClient) { }

  ngOnInit(): void {

  }
  registerToggle(){
    this.RegisterMode=!this.RegisterMode;
  }
  cancelRegisterMode(event:boolean){
    this.RegisterMode=event;
  }

}
