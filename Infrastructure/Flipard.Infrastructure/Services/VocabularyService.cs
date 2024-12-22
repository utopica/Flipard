using Flipard.Application.Repositories;
using Flipard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Flipard.Application.Services
{
    public class VocabularyService : IVocabularyService
    {
        private readonly IRepository<Vocabulary> _vocabularyRepository;

        public VocabularyService(IRepository<Vocabulary> vocabularyRepository)
        {
            _vocabularyRepository = vocabularyRepository;
        }

        public async Task<List<Vocabulary>> GetAllVocabulariesAsync()
        {
            return await _vocabularyRepository.GetAll().ToListAsync();
        }

        public async Task<Vocabulary?> GetVocabularyByIdAsync(Guid id)
        {
            return await _vocabularyRepository.GetByIdAsync(id.ToString());
        }

        public async Task<bool> AddVocabularyAsync(Vocabulary vocabulary)
        {
            await _vocabularyRepository.AddAsync(vocabulary);
            await _vocabularyRepository.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateVocabularyAsync(Vocabulary vocabulary)
        {
            _vocabularyRepository.Update(vocabulary);
            await _vocabularyRepository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteVocabularyAsync(Guid id)
        {
            return await _vocabularyRepository.DeleteAsync(id.ToString());
        }

        public async Task<List<Vocabulary?>> GetVocabulariesByCriteriaAsync(Expression<Func<Vocabulary, bool>> criteria)
        {
            return await _vocabularyRepository.GetWhere(criteria).ToListAsync();
        }
    }
}