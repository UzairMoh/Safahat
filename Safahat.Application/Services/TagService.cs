using System.Text.RegularExpressions;
using AutoMapper;
using Safahat.Application.DTOs.Requests.Tags;
using Safahat.Application.DTOs.Responses.Tags;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;

    public TagService(
        ITagRepository tagRepository,
        IMapper mapper)
    {
        _tagRepository = tagRepository;
        _mapper = mapper;
    }

    public async Task<TagResponse> GetByIdAsync(int id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            throw new ApplicationException("Tag not found");
        }

        return _mapper.Map<TagResponse>(tag);
    }

    public async Task<TagResponse> GetBySlugAsync(string slug)
    {
        var tag = await _tagRepository.GetBySlugAsync(slug);
        if (tag == null)
        {
            throw new ApplicationException("Tag not found");
        }

        return _mapper.Map<TagResponse>(tag);
    }

    public async Task<IEnumerable<TagResponse>> GetAllAsync()
    {
        var tags = await _tagRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TagResponse>>(tags);
    }

    public async Task<TagResponse> CreateAsync(CreateTagRequest request)
    {
        // Generate slug if not provided
        if (string.IsNullOrEmpty(request.Slug))
        {
            request.Slug = GenerateSlug(request.Name);
        }
        else
        {
            request.Slug = GenerateSlug(request.Slug);
        }

        // Check if slug is unique
        var isSlugUnique = await _tagRepository.IsSlugUniqueAsync(request.Slug);
        if (!isSlugUnique)
        {
            throw new ApplicationException("A tag with this slug already exists");
        }

        var tag = _mapper.Map<Tag>(request);
        var createdTag = await _tagRepository.AddAsync(tag);
        
        return _mapper.Map<TagResponse>(createdTag);
    }

    public async Task<TagResponse> UpdateAsync(int tagId, UpdateTagRequest request)
    {
        var tag = await _tagRepository.GetByIdAsync(tagId);
        if (tag == null)
        {
            throw new ApplicationException("Tag not found");
        }

        // Generate slug if name changed
        if (!string.IsNullOrEmpty(request.Name) && request.Name != tag.Name && string.IsNullOrEmpty(request.Slug))
        {
            request.Slug = GenerateSlug(request.Name);
        }
        else if (!string.IsNullOrEmpty(request.Slug))
        {
            request.Slug = GenerateSlug(request.Slug);
        }

        // Check if slug is unique (if changed)
        if (!string.IsNullOrEmpty(request.Slug) && request.Slug != tag.Slug)
        {
            var isSlugUnique = await _tagRepository.IsSlugUniqueAsync(request.Slug);
            if (!isSlugUnique)
            {
                throw new ApplicationException("A tag with this slug already exists");
            }
        }

        // Update tag properties
        _mapper.Map(request, tag);
        tag.UpdatedAt = DateTime.UtcNow;

        await _tagRepository.UpdateAsync(tag);
        return _mapper.Map<TagResponse>(tag);
    }

    public async Task<bool> DeleteAsync(int tagId)
    {
        var tag = await _tagRepository.GetByIdAsync(tagId);
        if (tag == null)
        {
            throw new ApplicationException("Tag not found");
        }

        // Check if tag is used in any posts before deletion
        // This might require a method in the post repository to check this

        await _tagRepository.DeleteAsync(tagId);
        return true;
    }

    public async Task<IEnumerable<TagResponse>> GetTagsWithPostCountAsync()
    {
        // Get all tags
        var tags = await _tagRepository.GetAllAsync();
        
        // Map to response DTOs (the PostCount property should be populated by mapping profile)
        return _mapper.Map<IEnumerable<TagResponse>>(tags);
    }

    public async Task<IEnumerable<TagResponse>> GetPopularTagsAsync(int count)
    {
        // Get all tags
        var tags = await _tagRepository.GetAllAsync();
        
        // Map to response DTOs
        var tagResponses = _mapper.Map<IEnumerable<TagResponse>>(tags);
        
        // Sort by post count and take the requested number
        return tagResponses
            .OrderByDescending(t => t.PostCount)
            .Take(count);
    }

    #region Helper Methods

    private string GenerateSlug(string text)
    {
        // Convert to lowercase
        string slug = text.ToLowerInvariant();
        
        // Remove diacritics (accents)
        slug = RemoveDiacritics(slug);
        
        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s", "-");
        
        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove multiple hyphens
        slug = Regex.Replace(slug, @"-+", "-");
        
        // Trim hyphens from start and end
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