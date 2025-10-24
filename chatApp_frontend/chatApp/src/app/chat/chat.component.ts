import { Component, OnInit, OnDestroy } from '@angular/core';
import { ChatService, Message } from '../services/chat.service';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { TestService } from '../services/test.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit, OnDestroy {
  messages: any[] = [];
  users: any[] = [];
  newMessage = '';
  selectedUser: any;
  isConnected = false;
  public loading: boolean = false;
  private messageSub?: Subscription;
  currentUserId: string = '';
  connectionState: string = 'Disconnected';
  isUserListOpen = false;
  isSettingsOpen = false;
  isDarkMode = false;
  isProfileEditOpen = false;
  currentUser: any = {};
  showSendButton = false;

  constructor(
    private chatService: ChatService,
    public authService: AuthService,
    private notificationService: NotificationService,
    private http: HttpClient,
    private testService: TestService
  ) { }

  onInput() {
    this.showSendButton = this.newMessage.trim().length > 0;
  }

  toggleUserList() {
    this.isUserListOpen = !this.isUserListOpen;
  }

  toggleSettings() {
    this.isSettingsOpen = !this.isSettingsOpen;
  }

  toggleProfileEdit() {
    this.isProfileEditOpen = !this.isProfileEditOpen;
  }

  toggleTheme() {
    this.isDarkMode = !this.isDarkMode;
    if (this.isDarkMode) {
      document.body.classList.add('dark-theme');
    } else {
      document.body.classList.remove('dark-theme');
    }
  }

  updateProfile() {
    this.authService.updateProfile(this.currentUser.id, this.currentUser).subscribe(() => {
      this.notificationService.showNotification('Profile Updated', {
        body: 'Your profile has been updated successfully.'
      });
    });
  }

  async ngOnInit(): Promise<void> {
    this.notificationService.requestPermission();

    // 1. Get current user ID
    const decodedToken = this.authService.getDecodedToken();
    if (decodedToken && decodedToken.UserId) {
      this.currentUserId = decodedToken.UserId;
      console.log('👤 Current User ID:', this.currentUserId);

      this.authService.getUser(this.currentUserId).subscribe(user => {
        this.currentUser = user.data;
      });

      // 2. Load users
      this.loadUsers();

      // 3. Initialize SignalR connection
      await this.initChatConnection();

      // 4. Subscribe to incoming messages
      this.messageSub = this.chatService.getMessageObservable().subscribe((message: any) => {
        this.handleIncomingMessage(message);
      });

      // 5. Periodically check connection status
      let lastConnectionStatus = this.isConnected;
      setInterval(() => {
        this.connectionState = this.chatService.getConnectionState();
        this.isConnected = this.chatService.connected;
        if (this.isConnected !== lastConnectionStatus) {
          if (this.isConnected) {
            this.notificationService.showNotification('Connection Status', {
              body: 'Reconnected to the chat server.'
            });
          } else {
            this.notificationService.showNotification('Connection Status', {
              body: 'Lost connection to the chat server.'
            });
          }
          lastConnectionStatus = this.isConnected;
        }
      }, 3000);

    } else {
      console.error('❌ No user token or UserId found in token');
    }
  }

  private handleIncomingMessage(message: any): void {
    console.log('📨 RAW message received in component:', message);
    console.log('🔍 Current selected user ID:', this.selectedUser?.id);
    console.log('🔍 Message senderId:', message.senderId);
    console.log('🔍 Message receiverId:', message.receiverId);
    console.log('🔍 Current user ID:', this.currentUserId);
    
    // IMPORTANT FIX: Check if this message is for the current user
    const isForCurrentUser = message.receiverId == this.currentUserId;
    const isFromSelectedUser = this.selectedUser && message.senderId == this.selectedUser.id;
    const isToSelectedUser = this.selectedUser && message.receiverId == this.selectedUser.id;
    const isMyOwnMessage = message.senderId == this.currentUserId;
    
    console.log('💬 Is for current user:', isForCurrentUser);
    console.log('💬 Is from selected user:', isFromSelectedUser);
    console.log('💬 Is to selected user:', isToSelectedUser);
    console.log('💬 Is my own message:', isMyOwnMessage);
    
    // Show message if:
    // 1. It's sent to current user AND from selected user, OR
    // 2. It's sent by current user AND to selected user, OR  
    // 3. It's a message in the current conversation
    const shouldShowMessage = 
      (isForCurrentUser && isFromSelectedUser) ||
      (isMyOwnMessage && isToSelectedUser) ||
      (this.selectedUser && 
        (message.senderId === this.selectedUser.id || 
         message.receiverId === this.selectedUser.id));
    
    console.log('💬 Should show message:', shouldShowMessage);
    
    if (shouldShowMessage) {
      console.log('✅ Adding message to current conversation');
      
      // Check if message already exists (to avoid duplicates)
      const messageExists = this.messages.some(msg => 
        msg.id === message.id || 
        (msg.content === message.content && 
         msg.senderId === message.senderId && 
         msg.receiverId === message.receiverId &&
         Math.abs(new Date(msg.timestamp).getTime() - new Date(message.timestamp).getTime()) < 1000)
      );
      
      if (!messageExists) {
        this.messages.push(message);
        this.notificationService.showNotification('📩 New Message', {
          body: `From ${this.getUserName(message.senderId)}: ${message.content}`
        });
        this.scrollToBottom();
      } else {
        console.log('⚠️ Message already exists, skipping duplicate');
      }
    } else {
      console.log('❌ Message not for current conversation, ignoring');
      // Show notification for messages not in current conversation
      if (isForCurrentUser && !isFromSelectedUser) {
        this.notificationService.showNotification('📩 New Message from ' + this.getUserName(message.senderId), {
          body: message.content
        });
      }
    }
  }

  async echo() {
    const response = await this.testService.echo("Hello from client");
    console.log(response);
  }

  private async initChatConnection(): Promise<void> {
    try {
      console.log('🔄 Starting SignalR connection for user:', this.currentUserId);
      const connected = await this.chatService.startConnection();
      this.isConnected = connected;
      this.connectionState = this.chatService.getConnectionState();
      
      if (connected) {
        console.log('✅ Chat hub connected for user:', this.currentUserId);
        this.notificationService.showNotification('Connection Status', {
          body: 'Connected to the chat server.'
        });
      } else {
        console.warn('⚠️ Chat hub connection failed for user:', this.currentUserId);
        this.notificationService.showNotification('Connection Status', {
          body: 'Failed to connect to the chat server.'
        });
        // Retry after 5 seconds
        setTimeout(() => this.initChatConnection(), 5000);
      }
    } catch (error) {
      console.error('❌ Initial hub connection failed:', error);
      this.isConnected = false;
      this.connectionState = 'Failed';
      this.notificationService.showNotification('Connection Status', {
        body: 'Error connecting to the chat server.'
      });
      
      // Retry after 5 seconds
      setTimeout(() => this.initChatConnection(), 5000);
    }
  }

  private loadUsers(): void {
    this.authService.getUsers().subscribe({
      next: (response) => {
        console.log('👥 Users loaded:', response);
        this.users = response.data || [];
        
        // Filter out current user from users list
        this.users = this.users.filter(user => user.id !== this.currentUserId);
        console.log('👥 Filtered users:', this.users);
      },
      error: (error) => {
        console.error('❌ Failed to load users:', error);
      }
    });
  }

  ngOnDestroy(): void {
    this.messageSub?.unsubscribe();
    this.chatService.stopConnection();
  }

  selectUser(user: any): void {
    console.log('✅ User selected:', user);
    this.selectedUser = user;
    this.messages = []; // Reset chat window
    
    if (this.currentUserId && this.selectedUser) {
      this.loadMessageHistory(this.currentUserId, this.selectedUser.id);
    }
  }

  loadMessageHistory(userId: string, recipientId: string): void {
    this.loading = true;
    console.log('📚 Loading message history between:', userId, 'and', recipientId);
    
    this.http.get(`https://localhost:7086/api/Message/GetMessages/${userId}/${recipientId}`)
      .subscribe({
        next: (response: any) => {
          console.log('📚 Message history loaded:', response);
          this.messages = response.data || [];
          this.loading = false;
          
          // Scroll to bottom after loading messages
          setTimeout(() => {
            this.scrollToBottom();
          }, 100);
        },
        error: (error) => {
          console.error('❌ Failed to load message history:', error);
          this.loading = false;
        }
      });
  }

  async sendMessage(): Promise<void> {
    console.log('=== SEND MESSAGE DEBUG ===');
    console.log('👤 Selected user:', this.selectedUser);
    console.log('👤 Current user ID:', this.currentUserId);
    console.log('💬 Message content:', this.newMessage);
    console.log('🔗 Is connected:', this.isConnected);
    console.log('🔗 Connection state:', this.connectionState);

    if (!this.newMessage.trim()) {
      console.warn('❌ Message is empty');
      return;
    }

    if (!this.selectedUser) {
      console.warn('❌ No user selected');
      alert('Please select a user to chat with');
      return;
    }

    if (!this.currentUserId) {
      console.warn('❌ User not authenticated');
      return;
    }

    // Create message object with CORRECT property names and types
    const message: Message = {
      content: this.newMessage.trim(),
      senderId: parseInt(this.currentUserId, 10),
      receiverId: parseInt(this.selectedUser.id, 10),
      timestamp: new Date()
    };

    console.log('📤 Final message object being sent:', message);

    try {
      // Add message to local UI immediately (optimistic update)
      const optimisticMessage = {
        ...message,
        id: 'temp-' + Date.now(),
        isOptimistic: true
      };
      this.messages.push(optimisticMessage);
      
      // Clear input immediately for better UX
      this.newMessage = '';
      
      // Scroll to bottom
      this.scrollToBottom();

      // Send to backend - this will work even if SignalR is disconnected
      console.log('🚀 Calling chatService.sendMessage...');
      const result = await this.chatService.sendMessage(message);
      console.log('✅ Backend response:', result);
      console.log('✅ Message sent successfully');
      this.notificationService.showNotification('Message Sent', {
        body: 'Your message has been sent successfully.'
      });

      // Remove the optimistic flag after successful send
      const messageIndex = this.messages.findIndex(msg => msg.isOptimistic && msg.id === optimisticMessage.id);
      if (messageIndex !== -1) {
        delete this.messages[messageIndex].isOptimistic;
      }

    } catch (error: any) {
      console.error('💥 Failed to send message:', error);
      
      // Remove optimistic message if send failed
      this.messages = this.messages.filter(msg => !msg.isOptimistic);
      
      // Show error to user
      const errorMessage = error?.error?.message || 'Failed to send message';
      console.error('🔍 Full error details:', error);
      alert(`Send failed: ${errorMessage}`);
    }
  }

  // Add this method to test the connection
  async testConnection(): Promise<void> {
    console.log('=== CONNECTION TEST ===');
    console.log('👤 Current User ID:', this.currentUserId);
    console.log('👤 Selected User ID:', this.selectedUser?.id);
    console.log('🔗 SignalR Connected:', this.isConnected);
    console.log('🔗 Connection State:', this.connectionState);
    
    if (this.selectedUser) {
      const testMessage: Message = {
        content: 'Test message at ' + new Date().toISOString(),
        senderId: parseInt(this.currentUserId, 10),
        receiverId: parseInt(this.selectedUser.id, 10),
        timestamp: new Date()
      };
      
      console.log('🧪 Sending test message:', testMessage);
      
      try {
        await this.chatService.sendMessage(testMessage);
        console.log('✅ Test message sent successfully');
      } catch (error) {
        console.error('❌ Test message failed:', error);
      }
    } else {
      console.warn('❌ No user selected for test');
    }
  }

  // Manual reconnection method
  async reconnect(): Promise<void> {
    console.log('🔄 Manual reconnection initiated');
    await this.chatService.reconnect();
    this.isConnected = this.chatService.connected;
    this.connectionState = this.chatService.getConnectionState();
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      const chatContainer = document.querySelector('.messages');
      if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
      }
    }, 100);
  }

  // Helper method to check if message is from current user
  isMyMessage(message: any): boolean {
    const isMine = message.senderId == this.currentUserId;
    console.log(`Message from ${message.senderId}, current user ${this.currentUserId}, isMine: ${isMine}`);
    return isMine;
  }

  // Format timestamp for display
  formatTimestamp(timestamp: string | Date): string {
    if (!timestamp) return '';
    
    const date = timestamp instanceof Date ? timestamp : new Date(timestamp);
    
    // Check if date is valid
    if (isNaN(date.getTime())) {
      return '';
    }
    
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  // Get user name by ID
  getUserName(userId: string): string {
    const user = this.users.find(u => u.id === userId);
    return user?.name || 'Unknown User';
  }
}