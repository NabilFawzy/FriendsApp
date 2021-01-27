import { Component, Input, OnInit ,EventEmitter} from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { User } from 'src/app/_models/User';

@Component({
  selector: 'app-roles-models',
  templateUrl: './roles-models.component.html',
  styleUrls: ['./roles-models.component.css']
})
export class RolesModelsComponent implements OnInit {
  @Input() updatedSelectedRoles=new EventEmitter();
  user:User;
  roles:any=[];


  constructor(public bsModalRef:BsModalRef) { }

  ngOnInit(): void {
  }
  updateRoles(){
    this.updatedSelectedRoles.emit(this.roles);
    this.bsModalRef.hide();
  }

}
