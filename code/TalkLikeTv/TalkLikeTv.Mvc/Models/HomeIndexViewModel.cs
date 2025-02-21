using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record HomeIndexViewModel(int VisitorCount,
    IList<Language> Languages );
