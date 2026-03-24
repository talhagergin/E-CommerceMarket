namespace ToptanciECommerce.Domain.Enums;

public enum OrderStatus
{
    Pending           = 0,
    PaymentProcessing = 1,
    PaymentSucceeded  = 2,
    PaymentFailed     = 3,
    Processing        = 4,
    Shipped           = 5,
    Delivered         = 6,
    Cancelled         = 7,
    Refunded          = 8,
    Listed            = 9   // Sipariş listesi PDF'i oluşturuldu
}

public static class OrderStatusExtensions
{
    public static string ToTurkish(this OrderStatus status) => status switch
    {
        OrderStatus.Pending           => "Beklemede",
        OrderStatus.PaymentProcessing => "Ödeme İşleniyor",
        OrderStatus.PaymentSucceeded  => "Ödeme Onaylandı",
        OrderStatus.PaymentFailed     => "Ödeme Başarısız",
        OrderStatus.Processing        => "Hazırlanıyor",
        OrderStatus.Shipped           => "Kargoda",
        OrderStatus.Delivered         => "Teslim Edildi",
        OrderStatus.Cancelled         => "İptal Edildi",
        OrderStatus.Refunded          => "İade Edildi",
        OrderStatus.Listed            => "Sipariş Listesi Oluşturuldu",
        _                             => status.ToString()
    };

    public static string BadgeClass(this OrderStatus status) => status switch
    {
        OrderStatus.Listed            => "bg-primary",
        OrderStatus.Processing        => "bg-info text-dark",
        OrderStatus.Shipped           => "bg-primary",
        OrderStatus.Delivered         => "bg-success",
        OrderStatus.Cancelled         
        or OrderStatus.PaymentFailed  => "bg-danger",
        OrderStatus.Refunded          => "bg-secondary",
        OrderStatus.PaymentProcessing => "bg-warning text-dark",
        _                             => "bg-warning text-dark"
    };
}
