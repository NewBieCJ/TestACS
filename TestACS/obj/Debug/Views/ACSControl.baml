   M S B A M L   `   `   `  ����B  >TestACS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:TestACS.ViewTestACS.View  5      7 clr-namespace:TestACS.ViewModelsTestACS.ViewModels  5      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5      7 clr-namespace:TestACS.ConvertersTestACS.Converters  5	      -clr-namespace:TestACS.RulesTestACS.Rules  5
      c _System.Windows.Interactivity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity 5      ^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35tOclr-namespace:System.Windows.Forms.Integration;assembly=WindowsFormsIntegration System.Windows.Forms.Integration 5      H DDocViewerDemo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullD2clr-namespace:DocViewerDemo;assembly=DocViewerDemoDocViewerDemo 5      7 clr-namespace:TestACS.ViewModelsTestACS.ViewModels  5          TestACS.View.ACSControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ,localclr-namespace:TestACS.View     5      /VM clr-namespace:TestACS.ViewModels     5      9System&clr-namespace:System;assembly=mscorlib     5      6	converter clr-namespace:TestACS.Converters     5	      ,ruleclr-namespace:TestACS.Rules     5
      ^iPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity     5      ^myOclr-namespace:System.Windows.Forms.Integration;assembly=WindowsFormsIntegration     5      FDocView2clr-namespace:DocViewerDemo;assembly=DocViewerDemo     5      1test clr-namespace:TestACS.ViewModels     5        � 	Resources  5      ,   &TestACS.Converters.ColorConverterGreen   ColorConverter(   "TestACS.Converters.StringConverter  StringConverter,  &TestACS.Converters.MotorStateConverter  MotorStateConverter,   &TestACS.ViewModels.ACSControlViewModel  ACSControlViewModel��%t   &	        &	      &	 &     &	 9       5   
   6(     5   
   6$     5   
   6(     5   
   6#   5      .��5      �  � RowDefinitions 5   
   �� 5      $��30 �6   �� 5      $��* �6   .+�5   
   �� $��Center=�6*   $��0��6E   ��6   �� *MyVar+6E   � 5"   
   $��0,25,0,5q�6   $��2��6!    � ColumnDefinitions 5$      �� 5%      $��* �6#   �� 5&      $��* �6#   .+�5(      � $��0��6    5)      �� 5*      $	��20* �6$   �� 5+      $
��100* �6$   �� 5,      $	��30* �6$   .+�5.      �� $��0��6    �� Orientation$ 
Horizontal=�6*   .+�5/      �� $
��10,0q�6   $
��Open��6+   $��20��6:   ��6F   �� *OpenSimulatorCMD+�� 50      $��10(�6   $��Center=�6*   $��20,0,0,0q�6E   $��Connect State��6W   A� 51      $��20��6   $��20��6*   
����  �$	��0,8q�6H   ��54      �� 55       �� Path$ IsOpened�6&    �� 	Converter 56   "     57   &   59      5<      5=      �� 5>      $��1��6   .+�5?      �� $ 
Horizontal=�6!   .+�5@      �� $��70��6$   $��10q�6/   $��Center=�6;    �� SelectedIndex 6V   �� *	AxisNum+��6x   �� *
AxesList+�� 5A      $��45��6!   $��25��6-   ��69   �� *
IsEnable+# [ ��6�   �� *EnableOperationCMD+�� 5B      $��50��6$   �� 5C      $��45��6"   $��25��6.   $
��Stop��6:   
���  ��$
��Bold"�6Z   ��6l   �� *StopMotionCMD+5D      �� 5E      $ 
Horizontal=�6!   $��10q�6:   .+�5F      �� $��50��6$   �� 5G      $��JOG +��6#   $��45��63   $��25��6?   .  (System.Windows.Interactivity.Interaction   Triggers 5I      /  )System.Windows.Interactivity.EventTrigger  5J   "      	EventName$  PreviewMouseLeftButtonDown��61   .  (System.Windows.Interactivity.TriggerBase	   Actions.	 5K   &   6  0System.Windows.Interactivity.InvokeCommandAction  
   Command
 6<   �� *
Jog_PCMD+5L   #     5M   "   $ PreviewMouseLeftButtonUp��61   .	 5N   &     
 6<   �� *StopMotionCMD+5O   #   5Q      �� 5R      $��50��6$   �� 5S      $��JOG -��6"   $��45��62   $��25��6>    5T        5U   "   $  PreviewMouseLeftButtonDown��61   .	 5V   &     
 6<   �� *
Jog_NCMD+5W   #     5X   "   $ PreviewMouseLeftButtonUp��61   .	 5Y   &     
 6<   �� *StopMotionCMD+5Z   #   5\      5]      �� 5^      $ 
Horizontal=�6!   .+�5_      �� $��Target Point��6$   $��10q�68   $��10(�6D   $��Center=�6R   �� 5`      $��60��6"   $��20��6-   .��69   .��5a   ?   ��5b      �� 5c   "   $ TargetPoint�6+    �� UpdateSourceTrigger$ Default=�6>    �� ValidationRules 5d   &   #	  TestACS.Rules.TargetPointRule	  5e   *   5g   #   .��5h   ,   5i      �� 5j      $��10��6$   �� 5k      $
��Auto��6#   $��Center=�60   $	��REL��6K    d� 	IsChecked 6Y   �� *IsRel+ �� Mode$ TwoWay=��� 5l      $��10��6$   �� 5m      $��Go��6!   $��45��6.   $��25��6:   ��6F   �� *
ToPointCMD+5n      �� 5o      $ 
Horizontal=�6!   $
��0,10q�6:   .+�5p      �� $��Motor State��6$   $��10q�67   $��10(�6C   �� 5q      $��20��6"   $	��170��6.   ��6:   �� *State+# [ 5r      5s      �� 5t      $��2��6   .+�5u      �� $��Postion:��6    $��10(�60   �� 5v      5�      �� 5�      $��0q�6   $��22��6)   $��Center=�6v    �� TextAlignment$ Center=�6�   ��65   �� *FeedbackPos+$ PropertyChanged=�6�   �� 5�      $��0q�6   $��Clear��6(   ��68   �� *ZeroFPostionCMD+5�      5�      7
  1System.Windows.Forms.Integration.WindowsFormsHost
  5�      -   5        $��windowsFormsHost1��5�   "   $��1��6;   $��5q�6K    
  Child. 5�      $  DocViewerDemo.ControlDocViewer  5�      �� 5�      $��1��6   $	��右��6$   $��Center=�60   $��Center=�6K   5�      5�      5�      