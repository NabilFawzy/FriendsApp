import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';
import { ConfirmService } from '../_services/confirm.service';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangedGuard implements CanDeactivate<unknown> {
 
  constructor(private confrimService:ConfirmService){

  }

  canDeactivate(
    component: MemberEditComponent):  Observable<boolean> |boolean {
      if(component.editForm.dirty){
        return this.confrimService.confirm();
      }
    return true;
  }
  
}
