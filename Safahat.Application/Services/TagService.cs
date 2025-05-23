using System.Text.RegularExpressions;
using AutoMapper;
using Safahat.Application.DTOs.Requests.Tags;
using Safahat.Application.DTOs.Responses.Tags;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Application.Services;

public class TagService(
    ITagRepository tagRepository,
    IMapper mapper) : ITagService
{
    public async Task<TagResponse> GetByIdAsync(Guid id)
    {
        var tag = await tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            throw new ApplicationException("Tag not found");
        }

        return mapper.Map<TagResponse>(tag);
    }

    public async Task<TagResponse> GetBySlugAsync(string slug)
    {
        var tag = await tagRepository.GetBySlugAsync(slug);
        if (tag == null)
        {
            throw new ApplicationException("Tag not found");
        }

        return mapper.Map<TagResponse>(tag);
    }

    public async Task<IEnumerable<TagResponse>> GetAllAsync()
    {
        var tags = await tagRepository.GetAllAsync();
        return mapper.Map<IEnumerable<TagResponse>>(tags);
    }

    public async Task<TagResponse> CreateAsync(CreateTagRequest request)
    {
        if (string.IsNullOrEmpty(request.Slug))
        {
            request.Slug = GenerateSlug(request.Name);
        }
        else
        {
            request.Slug = GenerateSlug(request.Slug);
        }

        var isSlugUnique = await tagRepository.IsSlugUniqueAsync(request.Slug);
        if (!isSlugUnique)
        {
            throw new ApplicationException("A tag with this slug already exists");
        }

        var tag = mapper.Map<Tag>(request);
        var createdTag = await tagRepository.AddAsync(tag);
        
        return mapper.Map<TagResponse>(createdTag);
    }

    public async Task<TagResponse> UpdateAsync(Guid tagId, UpdateTagRequest request)
    {
        var tag = await tagRepository.GetByIdAsync(tagId);
        if (tag == null)
        {
            throw new ApplicationException("Tag not found");
        }

        if (!string.IsNullOrEmpty(request.Name) && request.Name != tag.Name && string.IsNullOrEmpty(request.Slug))
        {
            request.Slug = GenerateSlug(request.Name);
        }
        else if (!string.IsNullOrEmpty(request.Slug))
        {
            request.Slug = GenerateSlug(request.Slug);
        }

        if (!string.IsNullOrEmpty(request.Slug) && request.Slug != tag.Slug)
        {
            var isSlugUnique = await tagRepository.IsSlugUniqueAsync(request.Slug);
            if (!isSlugUnique)
            {
                throw new ApplicationException("A tag with this slug already exists");
            }
        }

        mapper.Map(request, tag);
        tag.UpdatedAt = DateTime.UtcNow;

        await tagRepository.UpdateAsync(tag);
        return mapper.Map<TagResponse>(tag);
    }

    public async Task<bool> DeleteAsync(Guid tagId)
    {
        var tag = await tagRepository.GetByIdAsync(tagId);
        if (tag == null)
        {
            throw new ApplicationException("Tag not found");
        }

        await tagRepository.DeleteAsync(tagId);
        return true;
    }

    public async Task<IEnumerable<TagResponse>> GetTagsWithPostCountAsync()
    {
        var tags = await tagRepository.GetAllAsync();
        return mapper.Map<IEnumerable<TagResponse>>(tags);
    }

    public async Task<IEnumerable<TagResponse>> GetPopularTagsAsync(int count)
    {
        var tags = await tagRepository.GetAllAsync();
        var tagResponses = mapper.Map<IEnumerable<TagResponse>>(tags);
        
        return tagResponses
            .OrderByDescending(t => t.PostCount)
            .Take(count);
    }

    #region Helper Methods

    private string GenerateSlug(string text)
    {
        string slug = text.ToLowerInvariant();
        slug = RemoveDiacritics(slug);
        slug = Regex.Replace(slug, @"\s", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        
        return slug;
    }

    private string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    #endregion
}