   M S B A M L   `   `   `  ����D  @TDSClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:TDSClient.PropertiesTDSClient.Properties  5   
   %    TDSClient.Forms.frmActivityEdit   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   8	globalres"clr-namespace:TDSClient.Properties     5   
   X TXceed.Wpf.Toolkit, Version=2.5.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   	   -   5        $   TDSClient.Properties.Resources    strActivity � Title# Z  5   	   $	��300��6<   $	��340��6I   $��Global User Interface)�6U   $��11(�6x    � 
ResizeMode$ NoResize=�6�    � WindowStyle$ 
ToolWindow=�6�    � ShowInTaskbar .  .��5      � .+�5   
   �� -   5         ��Name$ lblRoute��5   s      strRouteName#��Z 6   $
��Left=�6F   $��1,96,0,0q�6a   $��86��6�   $��29��6�   $	��Top=�6�   $��Bottom=�6�   �� 5	   
   -   5         ��Name$ txtRoute��5	   _   $��21��6   $
��Left=�6    �� 
IsReadOnly . $��
92,103,0,0q�6K   $	��Top=�6q   $	��198��6�   �� 5
   
   -   5        	 ��Name$	 cmdRoute��5
   M   $��22��6   $
��Left=�6   $��295,102,0,0q�68   $	��Top=�6_   $��22��6w   .��5      �� .+�5      A� 
 �� Left$
 -5��6   
���   �$��2��68   $
��Left=�6C   $��2��6^   A� 5      $
 0��6   
���   �$��2��67   $
��Left=�6B   $��2��6]   A� 5      $
 5��6   
���   �$��2��67   $
��Left=�6B   $��2��6]   5      5      &   Xceed.Wpf.Toolkit.TimeSpanUpDown  5   
   -   5          Name$ startActivity��5   2   $��	92,68,0,0q�6   #   FractionalSecondsDigitsCount$ 0��6I   $��75��6k   $��24��6v   $	��Top=�6�   $
��Left=�6�   �� 5   
      strStartActivity#��Z 6   $
��Left=�6J   $��1,70,0,0q�6e   $	��Top=�6w   $��77��6�   �� 5   
   $��+��6   $
��Left=�6   $��	72,71,0,0q�67   $	��Top=�6J   $��17��6b   �� 5   
   -   5        $��checkBoxMapReferencePoint��5   a      strMarkOnTheMap#��Z 6   $��	6,149,0,0q�6N   $��17��6�   $	��Top=�6�   $
��Left=�6�   $	��128��6�   �� 5   
      strGridHeaderSpeedKmh#��Z 6   $
��Left=�6O   $��	1,176,0,0q�6j   $	��Top=�6}   $��81��6�   %  Xceed.Wpf.Toolkit.IntegerUpDown  5   
   -   5          Name$ speedUpDown��5   w   $��
92,176,0,0q�6   �  �Xceed.Wpf.Toolkit.NumericUpDown`1[[System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   	Increment$ 1X�61   �  �Xceed.Wpf.Toolkit.Primitives.UpDownBase`1[[System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   Minimum$ 0X�6?      Maximum$	 200X�6L      FormatString$ N��6Z   $��24��6k   $
��Left=�6�   $��75��6�   $	��Top=�6�   �� 5   
   -   5        $��btnOK��5         strbtnOK#��Z 6   $
��Left=�6P   $��185,235,0,0q�6k   $	��Top=�6�   $��75��6�   $��20��6�   �� 5   
   -	   5        $ lblReferencePoint��5   y      strReferencePoint#��Z 6   $
��Left=�6K   $��	0,124,0,0q�6f   $��86��6�   $��29��6�   $	��Top=�6�   $��Bottom=�6�   �� 5   
   -
   5        $
��lblX��5   +      lblX#��Z 5      }�. $
��Left=�6Y   $��Center=�6o   $��126,131,0,0q�5      $
��Left=�67   $��38��6R   $��22��6]   $	��Top=�6i   6�   �� 5   
   -   5        $��txtReferenceX��5   s   }�. $
��Left=�6$   $��Center=�6:   $��155,131,0,0q�6^   $
��Left=�6�   $��58��6�   $��22��6�   $	��Top=�6�   .��6�   06�   �� 5    
   -   5        $
��lblY��5!   %      lblY#��Z 5       }�. $
��Left=�6U   $��Center=�6k   $��127,149,0,0q�5!      $
��Left=�61   $��35��6L   $��22��6W   $	��Top=�6c   6}   �� 5"   
   -   5        $��txtReferenceY��5"   s   }�. $
��Left=�6$   $��Center=�6:   $��155,149,0,0q�6^   $
��Left=�6�   $��58��6�   $��22��6�   $	��Top=�6�   .��6�   06�   �� 5#   
   -   5         ��Name$ textBlockRouteName_Copy��5$   
   $
��Wrap=�5#      $��16��6)   $
��Left=�65   $��6,40,0,0q�6P   $	��Top=�5$   +   $��79��6C      strPlatformName#��Z 6O   �� 5%   
   -   5        $ txPlatformName��5%   3   $��20��6   $��87,36,-19,0q�6   $	��Top=�6K   $
��Left=�6c   $	��236��6~   5'      5(      