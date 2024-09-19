using System.Net.Mail;
using System.Net;

namespace Eshopping.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            //email cần gửi 
            var client = new SmtpClient("smtp.gmail.com", 587)  //nó có 2 cổng là 587 và 465(ko bảo mật bằng) 
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false, //ko sd khóa này như login 
                Credentials = new NetworkCredential("trandang211@gmail.com", "zbxhcdtjkhpszwme")  //ta tạo 1 tài khoản gmail và mk của nó 
                //Mật khẩu ứng dụng là mật mã gồm 16 chữ số cho phép một ứng dụng hoặc thiết bị kém an toàn truy cập vào Tài khoản Google của bạn. Chỉ những tài khoản đã bật tính năng Xác minh 2 bước mới có thể sử dụng mật khẩu ứng dụng.
                //ĐỂ LẤY MK ỨNG DỤNG CỦA TÀI KHOAN GMAIL: -vào tk google 
                //-chọn manage your gg account -> security -> 2 step vertification (bảo mật 2 bước) ->NHẬP MK MÁY TÍNH -> TIẾP TỤC 
                // hoặc vào link này để tạo mk ứng dựng : https://myaccount.google.com/apppasswords
                // đặt tên ứng dụng là "test mail" , sau đó nó trả về mk ứng dụng "zbxh cdtj khps zwme" , tức là các thiết bị khác muốn đăng nhập tài khoản gg mà ko muốn xác minh 2 bước thì có thể nhập mk ứng dụng 
                //tức là ta sẽ gửi mail bằng mai trandang211@gmail.com đó 
            };
            //khi ng dùng xác nhận đơn hàng , ta sẽ gửi mail cho Khách hàng từ cái này 
            return client.SendMailAsync(
                new MailMessage(from: "trandang211@gmail.com",
                                to: email,  //phương thức gửi : thông qua email 
                                subject,  //tiêu đề
                                message  //ND
                                ));
        }
    }
}
