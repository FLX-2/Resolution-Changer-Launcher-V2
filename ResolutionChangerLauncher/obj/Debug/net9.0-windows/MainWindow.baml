   M S B A M L   `   `   `  ����T  PResolutionChangerLauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:ResolutionChangerLauncherResolutionChangerLauncher  5   	   *    $ResolutionChangerLauncher.MainWindow   5   	   W SPresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   9local'clr-namespace:ResolutionChangerLauncher     5   	     � Title$!  Resolution Changer Launcher��5   	   $	��600��6-   $	��800��6:    � WindowStartupLocation$ CenterScreen=�5	   	   .��5
      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $&�� Resolution Changer Game Launcher��6!   $��24(�5      $
��Bold"�6"   $��0,0,0,20q�64   � 5   
   $��1��6    � ColumnDefinitions 5      �� 5      $��* �6#   �� 5      $
��Auto �6#   �� 5      $��* �6#   .+�5      �� $��0��6   $��Saved Game Profiles��6'   $��0,0,10,0q�6D   .��5      �  5       �� 5!      $��* �6(   �� 5"      $
��Auto �6(   .+�5%      �� -   5         ��Name$ ProfilesListBox��5%      $��0��67   $��5q�6D   5&      �� 5(      $��1��6!    �� Orientation$ 
Horizontal=�6.   $��Right=�6G   .+�5)      �� -   5         ��Name$ DeleteProfileButton��5)   !   $��Delete��6>   6O   5*      5+      5,      �� 5/      $��1��6   $��5��6+   $��Center=�65   $��Stretch=�6R   �� 52      $��2��6   $��Game Settings��6'   $��10,0,0,0q�6>   .��53      �  54      �� 55      $
��Auto �6(   �� 56      $
��Auto �6(   �� 57      $
��Auto �6(   �� 58      $
��Auto �6(   �� 59      $
��Auto �6(   �� 5:      $��* �6(   �� 5;      $
��Auto �6(   .+�5?      �� $��0��6    $��
Game Path:��6-   $��5q�6?   � 5@      $��1��6    5A      �� 5B      $��* �6/   �� 5C      $
��Auto �6/   �� 5D      $
��Auto �6/   .+�5F      �� -   5         ��Name$ GamePathTextBox��5F   "   $��0��6;    �� 
IsReadOnly$ False��6K   �� 5G      -   5        $ BrowseButton��5G   !   $��1��67   $��Browse��6G   6X   �� 5H      -   5        $ 	UwpButton��5H   !   $��2��64   $��UWP App��6D   $��5,0,0,0q�6n   	 � ToolTip$*	 $Select a UWP app (Windows Store app)��5I   !   5J      �� 5M      $��2��6    $��
Game Name:��6-   $��5q�6?   �� 5N      -   5        $ GameNameTextBox��5N      $��3��67   �� 5Q      $��4��6    $��Resolution:��6-   $��5q�6@   � 5R      $��5��6    5S      �� 5T      $
��Auto �6,   �� 5U      $
��Auto �6,   �� 5V      $��* �6,   .+�5Y      �� -   5        
 ��Name$
 ResolutionComboBox��5Y   #   $��0��6?   5Z   #   � 5\      $��1��6   $��0,10,0,0q�6,    5]      �� 5^   "   $��* �63   �� 5_   "   $
��Auto �63   �� 5`   "   $��* �63   .+�5c      �� -   5        $ WidthTextBox��5c   &   $��0��6<   5d   &   �� 5e      $��1��6(   $��x��68   $��Center=�6A   $	��5,0q�6\   �� 5f      -	   5        $ HeightTextBox��5f   &   $��2��6=   5g   &   5h      �� 5j      -
   5         ��Name$ RevertResolutionCheckBox��5j   #   $��2��6E   $��0,10,0,0q�6R   $(��"Revert resolution when game closes��5k   #    d� 	IsChecked$
 TrueY�6P   5l      �� 5o      $��6��6!   $ 
Horizontal=�6.   $��Right=�6G   .+�5p      �� -   5        $ SaveProfileButton��5p   !   $��Save Profile��6<   6S   �� 5q      -   5        $ CreateShortcutButton��5q   !   $��Create Shortcut��6?   6Y   �� 5r      -   5        $ LaunchButton��5r   !   $��Launch Game��67   6M   5s      5t      5u      5v      �� 5y   
   $��2��6   $��0,10,0,0q�6!   .@�5z      �� .��5{      �� -   5         ��Name$ StatusTextBlock��5{      $��Ready��65   5|      5}      5~      5      