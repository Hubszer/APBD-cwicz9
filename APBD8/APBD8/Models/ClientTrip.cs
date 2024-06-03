using System;
using System.Collections.Generic;

namespace APBD8.Models;

public class ClientTrip
{
    public int Id { get; set; }
    public int IdClient { get; set; }
    public int IdTrip { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }

    public virtual Client IdClientNavigation { get; set; }
    public virtual Trip IdTripNavigation { get; set; }
}