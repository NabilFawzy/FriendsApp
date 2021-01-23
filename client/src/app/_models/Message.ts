  export interface Message
    {
            id  :number ;
            senderId   :number ;
            senderUsername  :string  ;
            senderPhotoUrl  :string ;

            recipentId  :number ;
            recipentUsername  :number ;
            recipentPhotoUrl  :string ;

            content  :string ;
            dateRead?  :Date ;
            messageSent  :Date ;
 

    }