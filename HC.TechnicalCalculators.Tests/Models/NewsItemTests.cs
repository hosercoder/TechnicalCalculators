using HC.TechnicalCalculators.Src.Models;
using System.ComponentModel.DataAnnotations;

namespace HC.TechnicalCalculators.Tests.Models
{
    public class NewsItemTests
    {
        [Fact(Skip = "Work in progress")]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Act
            var newsItem = new NewsItem();

            // Assert
            Assert.Equal(string.Empty, newsItem.Id);
            Assert.Equal(string.Empty, newsItem.Title);
            Assert.Null(newsItem.Summary);
            Assert.Null(newsItem.Url);
            Assert.Null(newsItem.Source);
            Assert.Null(newsItem.Author);
            Assert.Equal(default(DateTime), newsItem.PublishedAt);
            Assert.Empty(newsItem.RelatedSymbols);
            Assert.Equal(0.0, newsItem.SentimentScore);
            // Assert.Equal(0.0, newsItem.Relevance);
            // Assert.Empty(newsItem.Categories);
            Assert.Empty(newsItem.Keywords);
            Assert.Null(newsItem.Language);
            //  Assert.Null(newsItem.Region);
            //  Assert.Equal(0, newsItem.ViewCount);
            //   Assert.Equal(0, newsItem.ShareCount);
            //   Assert.False(newsItem.IsBreaking);
            //    Assert.False(newsItem.IsPremium);
            //    Assert.Equal(default(DateTime), newsItem.CreatedAt);
            //    Assert.Equal(default(DateTime), newsItem.UpdatedAt);
        }

        [Fact(Skip = "Work in progress")]
        public void Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var newsItem = new NewsItem();
            var testDate = DateTime.UtcNow;
            var relatedSymbols = new List<string> { "AAPL", "MSFT" };
            var categories = new List<string> { "Technology", "Earnings" };
            var keywords = new List<string> { "iPhone", "quarterly" };

            // Act
            newsItem.Id = "test-id-123";
            newsItem.Title = "Test News Title";
            newsItem.Summary = "Test summary content";
            newsItem.Url = "https://example.com/news";
            newsItem.Source = "Test Source";
            newsItem.Author = "Test Author";
            newsItem.PublishedAt = testDate;
            newsItem.RelatedSymbols = relatedSymbols;
            newsItem.SentimentScore = 0.75;
            //  newsItem.Relevance = 0.9;
            //  newsItem.Categories = categories;
            newsItem.Keywords = keywords;
            newsItem.Language = "en";
            //   newsItem.Region = "US";
            //   newsItem.ViewCount = 1000;
            //   newsItem.ShareCount = 50;
            //   newsItem.IsBreaking = true;
            //   newsItem.IsPremium = true;
            //   newsItem.CreatedAt = testDate;
            //   newsItem.UpdatedAt = testDate.AddMinutes(5);

            // Assert
            Assert.Equal("test-id-123", newsItem.Id);
            Assert.Equal("Test News Title", newsItem.Title);
            Assert.Equal("Test summary content", newsItem.Summary);
            Assert.Equal("https://example.com/news", newsItem.Url);
            Assert.Equal("Test Source", newsItem.Source);
            Assert.Equal("Test Author", newsItem.Author);
            Assert.Equal(testDate, newsItem.PublishedAt);
            Assert.Equal(relatedSymbols, newsItem.RelatedSymbols);
            Assert.Equal(0.75, newsItem.SentimentScore);
            //   Assert.Equal(0.9, newsItem.Relevance);
            //   Assert.Equal(categories, newsItem.Categories);
            Assert.Equal(keywords, newsItem.Keywords);
            Assert.Equal("en", newsItem.Language);
            //   Assert.Equal("US", newsItem.Region);
            //   Assert.Equal(1000, newsItem.ViewCount);
            //    Assert.Equal(50, newsItem.ShareCount);
            //    Assert.True(newsItem.IsBreaking);
            //   Assert.True(newsItem.IsPremium);
            //   Assert.Equal(testDate, newsItem.CreatedAt);
            //   Assert.Equal(testDate.AddMinutes(5), newsItem.UpdatedAt);
        }

        [Fact(Skip = "Work in progress")]
        public void Validation_ShouldValidateRequiredFields()
        {
            // Arrange
            var newsItem = new NewsItem
            {
                Id = "",  // Invalid - required
                Title = "" // Invalid - required
            };

            var validationContext = new ValidationContext(newsItem);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(newsItem, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Id"));
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Title"));
        }

        [Fact(Skip = "Work in progress")]
        public void Validation_ShouldValidateStringLengths()
        {
            // Arrange
            var newsItem = new NewsItem
            {
                Id = new string('a', 101), // Too long
                Title = new string('b', 501), // Too long
                Summary = new string('c', 2001), // Too long
                Source = new string('d', 101), // Too long
                Author = new string('e', 201) // Too long
            };

            var validationContext = new ValidationContext(newsItem);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(newsItem, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Id"));
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Title"));
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Summary"));
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Source"));
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Author"));
        }

        [Fact(Skip = "Work in progress")]
        public void Validation_ShouldValidateUrlFormat()
        {
            // Arrange
            var newsItem = new NewsItem
            {
                Id = "valid-id",
                Title = "Valid Title",
                Url = "invalid-url-format"
            };

            var validationContext = new ValidationContext(newsItem);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(newsItem, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Url"));
        }

        [Fact(Skip = "Work in progress")]
        public void Validation_ShouldPassWithValidData()
        {
            // Arrange
            var newsItem = new NewsItem
            {
                Id = "valid-id",
                Title = "Valid News Title",
                Summary = "Valid summary",
                Url = "https://example.com/news",
                Source = "Valid Source",
                Author = "Valid Author",
                PublishedAt = DateTime.UtcNow,
                SentimentScore = 0.5,
                //   Relevance = 0.8,
                Language = "en",
                //   Region = "US"
            };

            var validationContext = new ValidationContext(newsItem);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(newsItem, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact(Skip = "Work in progress")]
        public void SentimentScore_ShouldAllowNegativeAndPositiveValues()
        {
            // Arrange & Act
            var newsItem = new NewsItem { SentimentScore = -0.85 };

            // Assert
            Assert.Equal(-0.85, newsItem.SentimentScore);

            // Act
            newsItem.SentimentScore = 0.95;

            // Assert
            Assert.Equal(0.95, newsItem.SentimentScore);
        }

        [Fact(Skip = "Work in progress")]
        public void Collections_ShouldBeInitializedAsEmpty()
        {
            // Act
            var newsItem = new NewsItem();

            // Assert
            Assert.NotNull(newsItem.RelatedSymbols);
            Assert.Empty(newsItem.RelatedSymbols);
            //  Assert.NotNull(newsItem.Categories);
            //  Assert.Empty(newsItem.Categories);
            Assert.NotNull(newsItem.Keywords);
            Assert.Empty(newsItem.Keywords);
        }

        [Fact(Skip = "Work in progress")]
        public void Collections_ShouldSupportAddingElements()
        {
            // Arrange
            var newsItem = new NewsItem();

            // Act
            newsItem.RelatedSymbols.Add("AAPL");
            //  newsItem.Categories.Add("Technology");
            newsItem.Keywords.Add("iPhone");

            // Assert
            Assert.Single(newsItem.RelatedSymbols);
            Assert.Contains("AAPL", newsItem.RelatedSymbols);
            //    Assert.Single(newsItem.Categories);
            //    Assert.Contains("Technology", newsItem.Categories);
            Assert.Single(newsItem.Keywords);
            Assert.Contains("iPhone", newsItem.Keywords);
        }

        [Fact(Skip = "ShareCount is not implemented yet")]
        public void ViewCount_ShouldAcceptNonNegativeValues()
        {
            // Arrange & Act
            // var newsItem = new NewsItem { ViewCount = viewCount };

            // Assert
            //  Assert.Equal(viewCount, newsItem.ViewCount);
        }


        [Fact(Skip = "ShareCount is not implemented yet")]
        public void ShareCount_ShouldAcceptNonNegativeValues()
        {
            // Arrange & Act
            //  var newsItem = new NewsItem { ShareCount = shareCount };

            // Assert
            //  Assert.Equal(shareCount, newsItem.ShareCount);
        }
    }
}
