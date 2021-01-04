import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { Router } from '@angular/router';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
 
})
export class RegisterComponent implements OnInit {
 // @Input() usersFromHomeComponent:any;
 registerForm:FormGroup; 
 bsConfig: Partial<BsDatepickerConfig>;//date picker
 maxDate:Date;
 validationErrors:string[]=[];
  @Output() cancelRegister=new EventEmitter();
 
  constructor(public accountService:AccountService,private toastr:ToastrService
    ,private fb:FormBuilder,private router:Router) { }
   
  ngOnInit(): void {
    this.initializeForm();
    this.maxDate=new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear()-18);
  }
   
  initializeForm(){
    
    //datepicker
    this.bsConfig =  { 
      containerClass: "theme-blue",
      dateInputFormat:'DD MMMM YYYY'
     };
    this.registerForm=this.fb.group({
      gender:['male'],
      username:['',Validators.required],
      knownAs:['',Validators.required],
      dateOfBirth:['',Validators.required],
      city:['',Validators.required],
      country:['',Validators.required],
      password:['',
      [Validators.required,Validators.minLength(4),Validators.maxLength(8)]],
      confirmPassword:['',
      [Validators.required,this.matchValues('password')]]
    })
  }

  
  private matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
     
      if (control.parent && control.parent.controls) {
        return control?.value ===
          (control?.parent?.controls as { [key: string]: AbstractControl })[ matchTo ].value ? null : { isMatching: true };
      }

      return null;
    };
  }

 


  register(){
     this.accountService.register(this.registerForm.value).subscribe(Response=>{
       this.router.navigateByUrl('/members')
       this.cancel();
     },error=>{
       this.validationErrors=error
     }) 
  }
 
 
  cancel(){
    this.cancelRegister.emit(false);
  }

}
