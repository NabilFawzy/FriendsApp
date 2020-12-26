import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangedGuard implements CanDeactivate<unknown> {
  canDeactivate(
    component: MemberEditComponent):  boolean  {
      if(component.editForm.dirty){
        return confirm('Are you sure to continue? Any unsaved changed will be lost')
      }
    return true;
  }
  
}
