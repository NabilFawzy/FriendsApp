import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/Message';
import { Pagination } from '../_models/pagination';
import { ConfirmService } from '../_services/confirm.service';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages:Message[];
  pagination:Pagination;
  container='Unread';
  pageNumber=1;
  pageSize=5;
  laoding=false;
  constructor(private messageSerrvice:MessageService,private confirmService:ConfirmService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages(){
    this.laoding=true;
    this.messageSerrvice.getMessages(this.pageNumber,this.pageSize,this.container)
    .subscribe(response=>{
      this.messages=response.result;
      this.pagination=response.pagination;
      this.laoding=false;
    });
  }

  pageChanged(event:any){
    this.pageNumber=event.page;
    this.loadMessages();
  }

  deleteMessage(id:number){
    this.confirmService.confirm('Confirm delete message','this cannot be undone')
        .subscribe(result=>{
          if(result){
            this.messageSerrvice.deleteMessage(id).subscribe(()=>{
              this.messages.splice(this.messages.findIndex(m=>m.id===id),1);
            });
          }
        });
       
  }

}
