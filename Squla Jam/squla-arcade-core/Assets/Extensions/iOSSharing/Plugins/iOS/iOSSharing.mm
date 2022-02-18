@implementation ViewController : UIViewController

-(void) shareOnlyTextMethod: (const char *) shareMessage
{
    NSString *message   = [NSString stringWithUTF8String:shareMessage];
    NSArray *postItems  = @[message];
    
    UIActivityViewController *activityVc = [[UIActivityViewController alloc] initWithActivityItems:postItems applicationActivities:nil];
    
    
    if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad &&  [activityVc respondsToSelector:@selector(popoverPresentationController)] ) {
        
        UIPopoverController *popup = [[UIPopoverController alloc] initWithContentViewController:activityVc];
        
        [popup presentPopoverFromRect:CGRectMake(self.view.frame.size.width/2, self.view.frame.size.height/4, 0, 0)
                               inView:[UIApplication sharedApplication].keyWindow.rootViewController.view permittedArrowDirections:UIPopoverArrowDirectionAny animated:YES];
    }
    else
        [[UIApplication sharedApplication].keyWindow.rootViewController presentViewController:activityVc animated:YES completion:nil];
}

@end

extern "C"{
    void _shareOnlyTextMethod(const char * message){
        ViewController *vc = [[ViewController alloc] init];
        [vc shareOnlyTextMethod: message];
    }
}
