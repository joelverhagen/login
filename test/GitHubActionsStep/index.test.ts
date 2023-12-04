import * as main from '../../src/GitHubActionsStep/main'

const runMock = jest.spyOn(main, 'run').mockImplementation()

describe('index', () => {
  it('calls run when imported', async () => {
    require('../../src/GitHubActionsStep/index')

    expect(runMock).toHaveBeenCalled()
  })
})
