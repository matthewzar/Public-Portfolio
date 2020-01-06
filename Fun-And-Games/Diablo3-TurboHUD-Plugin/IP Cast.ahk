#SingleInstance,Force


CoordMode,Pixel,Screen

;PixelSearch, OutputVarx, OutputVarY, X1, Y1, X2, Y2, ColorID [, Varation, Fast|RGB]
-::

Loop  
{
    PixelSearch,ax,ay,945,355,970,362,0x00FA00,5,RGB Fast
    if ErrorLevel =0 
         
         {
          send 1 
          sleep 10900
         }
}
    return 

=::Pause, Toggle
^ESC::ExitApp



