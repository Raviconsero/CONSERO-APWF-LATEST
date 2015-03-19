// this code is based on the example here:
// https://developers.fogbugz.com/default.asp?W55
// it creates a standard FogBugz popup dialog with an empty form. It then fetches the dialog's
// html from FogBugz using jQuery.get(). RawPageDisplay() in the C# code generates this in
// geteditdialog. The callback here, postCallback, updates teh dialog and sets the correct
// onclick's for the ok and cancel buttons (overriding the FogBugz defaults)



var ixProject1="";


var ExamplePlugin = new function(){
    var oSelf = this;

    this.doPopup = function(elPopupAnchor)
    {
   
        oSelf.Popup.setHtml('<form action="default.asp" method="post" enctype="multipart/form-data" style="width:430px" id="ExamplePluginForm" onsubmit="return false;"><p id="pLoading" class="dialog-title">Loading...</p></form>');
        oSelf.Popup.showPopup(elPopupAnchor);
        //alert("Code reached upto this Java Script");
        var ixLineItem = elPopupAnchor.attributes.getNamedItem('ixLineItem').value;
        //var ixBug = elPopupAnchor.attributes.getNamedItem('ixBug').value;
        var sTableId =elPopupAnchor.attributes.getNamedItem('sTableId').value;
        
var ixProject = elPopupAnchor.attributes.getNamedItem('ixProject').value;
 ixProject1=ixProject;
       // alert(ixLineItem);
      // alert("Table Id=" + sTableId);
        jQuery.get(GetEditDialogUrl(ixLineItem,sTableId,ixProject), function(data) { postCallback(data); });
        
//alert("J1");
        return false;
    }
    
    $(document).ready(function(){
    //alert("Reached upto point4");
        oSelf.Popup = api.PopupManager.newPopup("ExamplePlugin");
    });
    
    function postCallback(data) {
        $('#pLoading').remove();
        $('form#ExamplePluginForm').append(data);
       // alert("Reached upto point1");
        //$("#dtDateOfBirth").blur();
        DropListControl.refreshWithin($("#ExamplePluginForm"));
       // alert("Reached upto point2");
        //$("#ExamplePluginForm").find('input[chotkey="o"]').attr("onclick","return ! submitEdit();");
        $("#ExamplePluginForm").find('input[chotkey="o"]').live('click', function(e){
            e.preventDefault();
            submitEdit();
        });
        $("#ExamplePluginForm").find('input[chotkey="c"]').live('click', function(e){
            e.preventDefault();
            ExamplePlugin.Popup.hide();
        });
        //$("#ExamplePluginForm").find('input[chotkey="c"]').attr("onclick","return ! ExamplePlugin.Popup.hide();");
       // alert("Reached upto point3");
    }
}();

// this method grabs the form fields and posts them to FogBugz where RawPageDisplay() handles updating
// the kiwi. The xml Response is then fed to the EditableTableManager to update the table with
// the new values. Finally, the popup is hidden.
function submitEdit(){

//alert("reached upto submit edit");

//  alert(ixProject1)

if(ixProject1==19)
{

//alert("project1")
  window.FOOxmlRespone;
    $.ajax({
        url: "default.asp",
        type: "POST",
        data: $("#ExamplePluginForm").serialize(),
        success: function(data){
       // alert('response received');
            xmlResponse = data;
            EditableTableManager.result('itemtable_1', xmlResponse);
            ExamplePlugin.Popup.hide();
        }
    });
    /*$.post("default.asp",
           $("#ExamplePluginForm").serialize(),
           function(data) {
         //  alert("Reached upto function data");
	            xmlResponse = data;

	            EditableTableManager.result('ItemTable_Cambridge', xmlResponse)

	            ExamplePlugin.Popup.hide();
           });*/

}

else if(ixProject1==23)
{

//alert("project1")
  window.FOOxmlRespone;
    $.ajax({
        url: "default.asp",
        type: "POST",
        data: $("#ExamplePluginForm").serialize(),
        success: function(data){
       // alert('response received');
            xmlResponse = data;
            EditableTableManager.result('itemtable_2', xmlResponse);
            ExamplePlugin.Popup.hide();
        }
    });
    /*$.post("default.asp",
           $("#ExamplePluginForm").serialize(),
           function(data) {
         //  alert("Reached upto function data");
	            xmlResponse = data;

	            EditableTableManager.result('ItemTable_TE', xmlResponse)

	            ExamplePlugin.Popup.hide();
           });*/

}

else if(ixProject1==25)
{

//alert("project1")
  window.FOOxmlRespone;
    $.ajax({
        url: "default.asp",
        type: "POST",
        data: $("#ExamplePluginForm").serialize(),
        success: function(data){
       // alert('response received');
            xmlResponse = data;
            EditableTableManager.result('itemtable_3', xmlResponse);
            ExamplePlugin.Popup.hide();
        }
    });
    /*$.post("default.asp",
           $("#ExamplePluginForm").serialize(),
           function(data) {
         //  alert("Reached upto function data");
	            xmlResponse = data;

	            EditableTableManager.result('ItemTable_synergs', xmlResponse)

	            ExamplePlugin.Popup.hide();
           });*/

}

else if(ixProject1==26)
{

//alert("project1")
  window.FOOxmlRespone;
    $.ajax({
        url: "default.asp",
        type: "POST",
        data: $("#ExamplePluginForm").serialize(),
        success: function(data){
       // alert('response received');
            xmlResponse = data;
            EditableTableManager.result('itemtable_4', xmlResponse);
            ExamplePlugin.Popup.hide();
        }
    });
    /*$.post("default.asp",
           $("#ExamplePluginForm").serialize(),
           function(data) {
         //  alert("Reached upto function data");
	            xmlResponse = data;

	            EditableTableManager.result('ItemTable_synergs_Artium', xmlResponse)

	            ExamplePlugin.Popup.hide();
           });*/

}

else if(ixProject1==27)
{

//alert("project1")
  window.FOOxmlRespone;
    $.ajax({
        url: "default.asp",
        type: "POST",
        data: $("#ExamplePluginForm").serialize(),
        success: function(data){
       // alert('response received');
            xmlResponse = data;
            EditableTableManager.result('itemtable_5', xmlResponse);
            ExamplePlugin.Popup.hide();
        }
    });
    /*$.post("default.asp",
           $("#ExamplePluginForm").serialize(),
           function(data) {
         //  alert("Reached upto function data");
	            xmlResponse = data;

	            EditableTableManager.result('ItemTable_Spreadfast', xmlResponse)

	            ExamplePlugin.Popup.hide();
           });*/

}



else
{
//alert("project2")
  window.FOOxmlRespone;
    $.ajax({
        url: "default.asp",
        type: "POST",
        data: $("#ExamplePluginForm").serialize(),
        success: function(data){
       // alert('response received');
            xmlResponse = data;
            EditableTableManager.result('itemtable', xmlResponse);
            ExamplePlugin.Popup.hide();
        }
    });
    /*$.post("default.asp",
           $("#ExamplePluginForm").serialize(),
           function(data) {
          // alert("Reached upto function data");
	            xmlResponse = data;

	            EditableTableManager.result('ItemTable', xmlResponse)

	            ExamplePlugin.Popup.hide();
           });*/
}
}

