namespace DevDay.Web.Models;

public readonly record struct UserQuestion(
    string Question,
    DateTime AskedOn);
