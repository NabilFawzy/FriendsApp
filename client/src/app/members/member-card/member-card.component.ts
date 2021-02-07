import { Component, Input, OnInit } from '@angular/core';
import { Toast, ToastrService } from 'ngx-toastr';
import { map } from 'rxjs/operators';
import { Member } from 'src/app/_models/Member';
import { MembersService } from 'src/app/_services/members.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  @Input() member:Member;
  users:string[];
  constructor(private memberService:MembersService,private toastr:ToastrService,
    public presenceService:PresenceService) {
      
   }

   addLike(member:Member){
       this.memberService.addLike(member.userName).subscribe(()=>{
         this.toastr.success("You have liked "+member.knownAs);
       });
   }

  ngOnInit(): void {
      
    this.getOnlineUsers();
  }
  getOnlineUsers(){
    this.presenceService.onlineUsers$
    .subscribe((result)=>{
      this.users=result;
    })
  }
  

}
