import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/Member';
import { Message } from 'src/app/_models/Message';
import { User } from 'src/app/_models/User';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit ,OnDestroy{
  @ViewChild('memberTabs',{static:true}) memberTabs:TabsetComponent;
 member:Member;
 galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  activTab:TabDirective;
  messages:Message[]=[];
  users:string[];
  user:User;
  constructor(private memberService:MembersService,private router:ActivatedRoute,
    private messageService:MessageService,public presenceService:PresenceService,
    private accountService:AccountService,private mrouter:Router) { 
      this.accountService.currentUser$.pipe(take(1)).subscribe(user=>{
        this.user=user;
        this.mrouter.routeReuseStrategy.shouldReuseRoute=()=>false;
        
      })
    }

 

  ngOnInit(): void {
    this.getOnlineUsers();
    this.router.data.subscribe(data=>{
      this.member=data.member;
    })

    this.router.queryParams.subscribe(params=>{
      params.tab?this.selectTab(params.tab):this.selectTab(0)
    })
    this.galleryOptions=[
      {
        width:"500px",
        height:"500px",
        imagePercent:100,
        thumbnailsColumns:4,
        imageAnimation:NgxGalleryAnimation.Slide,
        preview:false
      }
    ]
     
    this.galleryImages=this.getImages();
  }
  getOnlineUsers(){
    this.presenceService.onlineUsers$
    .subscribe((result)=>{
      this.users=result;
      
    })
  }

  getImages():NgxGalleryImage[]{
    const ImageUrl=[]
    for(const photo of this.member.photos){
      ImageUrl.push({
        small:photo?.url,
        medium:photo?.url,
        big:photo?.url
      });
    }
    return ImageUrl;
  }


  
  loadMessages(){
    this.messageService.getMessageThread(this.member.userName).subscribe( messages=>{
      this.messages=messages
    });
  }

  selectTab(tabId:number){
     this.memberTabs.tabs[tabId].active=true;
  }

  onTabActivated(data:TabDirective){
    this.activTab=data;
    if(this.activTab.heading==='Messages' && this.messages.length===0){
     // this.loadMessages();
     this.messageService.createHubConnection(this.user,this.member.userName);
    }
    else{
      this.messageService.stopHubConnection();
    }
  }
  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
 

}
