;#DropDB WingHinPortalEasyTest

#Application WingHinPortal.Win
#Application WingHinPortal.Web

*FillForm
 User Name = Admin
*Action Log In

#IfDef WingHinPortal.Win

*Action About...
 
*Action OK

#EndIf

#IfDef WingHinPortal.Web
*Action Navigation(My Details)
#EndIf
