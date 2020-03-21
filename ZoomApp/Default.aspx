<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ZoomApp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Zoom Step 4 -->
    <script>
        ZoomMtg.setZoomJSLib('https://source.zoom.us/1.7.2/lib', '/av'); 

        ZoomMtg.preLoadWasm();
        ZoomMtg.prepareJssdk();

        setTimeout(function () {
            ZoomMtg.init({
                leaveUrl: 'https://yoursite.com/meetingEnd',
                isSupportAV: true,
                isShowJoiningErrorDialog: false,
                success: function () {
                    ZoomMtg.join({
                        signature: '<%= Sig %>',
                        apiKey: '<%= ApiKey %>',
                        meetingNumber: '<%= MeetingNumber %>',
                        userName: '<%= UserName %>',
                        success: function (success) {
                            console.log(success)
                        },
                        error: function (error) {
                            console.log(error)
                            location.href = '<%= MeetingUrl %>';
                        }
                    })
                },
                error: function (error) {
                    console.log(error)
                }
            })
        }, 2000);
    </script>
</asp:Content>
